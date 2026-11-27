using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Tests.Unit;

public class VendorDocumentStorageTests
{
    [Fact]
    public void Migration_snapshot_matches_runtime_identity_configuration()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
            "Host=localhost;Database=design_time", npgsql => npgsql.MigrationsAssembly("WeddingPlanner.Web")));
        services.AddDefaultIdentity<User>().AddEntityFrameworkStores<ApplicationDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(context.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task Upload_download_and_delete_keep_metadata_and_use_random_key()
    {
        var fixture = new Fixture();
        var document = await fixture.Upload();
        Assert.Null(document.FileUrl);
        Assert.NotEqual(document.FileName, document.StorageKey);
        Assert.Equal("contract.pdf", document.FileName);
        Assert.Equal(fixture.Content.Length, document.FileSize);
        await using var downloaded = await fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "coordinator", default);
        using var buffer = new MemoryStream();
        await downloaded.CopyToAsync(buffer);
        Assert.Equal(fixture.Content, buffer.ToArray());
        await fixture.Service.DeleteStoredAsync(fixture.VendorId, document.Id, "coordinator", default);
        Assert.Empty(fixture.Documents.Items);
        Assert.Empty(fixture.Storage.Objects);
    }

    [Theory]
    [InlineData("contract.exe", "application/pdf", "%PDF-123")]
    [InlineData("contract.pdf", "image/png", "%PDF-123")]
    [InlineData("contract.pdf", "application/pdf", "not a pdf")]
    [InlineData("../contract.pdf", "application/pdf", "%PDF-123")]
    public async Task Invalid_files_are_rejected(string name, string contentType, string contents)
    {
        var fixture = new Fixture();
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
            fixture.Document(), new MemoryStream(Encoding.UTF8.GetBytes(contents)), name, contentType, "coordinator", default));
        Assert.Empty(fixture.Storage.Objects);
    }

    [Fact]
    public async Task File_size_limit_is_enforced()
    {
        var fixture = new Fixture();
        fixture.Options.MaxFileSizeBytes = 4;
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Upload());
        Assert.Empty(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Other_weddings_and_users_cannot_access_uploaded_files()
    {
        var fixture = new Fixture();
        var document = await fixture.Upload();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "other", default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.DeleteStoredAsync(Guid.NewGuid(), document.Id, "coordinator", default));
        var otherAssignment = new VendorAssignment { VendorId = fixture.VendorId, WeddingId = Guid.NewGuid() };
        fixture.Assignments.Items.Add(otherAssignment.Id, otherAssignment);
        var foreign = fixture.Document();
        foreign.VendorAssignmentId = otherAssignment.Id;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.UploadAsync(
            foreign, new MemoryStream(fixture.Content), "contract.pdf", "application/pdf", "coordinator", default));
        Assert.Single(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Failed_metadata_save_compensates_upload()
    {
        var fixture = new Fixture();
        fixture.Documents.FailAdd = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Upload());
        Assert.Empty(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Failed_compensation_is_reported_and_leaves_object_for_recovery()
    {
        var fixture = new Fixture();
        fixture.Documents.FailAdd = true;
        fixture.Storage.FailDelete = true;
        var error = await Assert.ThrowsAsync<AggregateException>(() => fixture.Upload());
        Assert.Equal(2, error.InnerExceptions.Count);
        Assert.Single(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Failed_database_update_never_deletes_the_object()
    {
        var fixture = new Fixture();
        var document = await fixture.Upload();
        fixture.Documents.FailUpdate = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.DeleteStoredAsync(fixture.VendorId, document.Id, "coordinator", default));
        Assert.Single(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Download_storage_failure_is_not_hidden()
    {
        var fixture = new Fixture();
        var document = await fixture.Upload();
        fixture.Storage.Objects.Clear();
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "coordinator", default));
    }

    [Fact]
    public async Task Storage_failures_are_reported_and_failed_deletion_can_be_retried()
    {
        var fixture = new Fixture();
        fixture.Storage.FailUpload = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Upload());
        Assert.Empty(fixture.Documents.Items);
        fixture.Storage.FailUpload = false;
        var document = await fixture.Upload();
        fixture.Storage.FailDelete = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.DeleteStoredAsync(fixture.VendorId, document.Id, "coordinator", default));
        Assert.True(document.DeletionPending);
        await Assert.ThrowsAsync<ValidationException>(() =>
            fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "coordinator", default));
        fixture.Storage.FailDelete = false;
        await fixture.Service.DeleteStoredAsync(fixture.VendorId, document.Id, "coordinator", default);
        Assert.Empty(fixture.Documents.Items);
    }

    [Fact]
    public async Task External_links_remain_unchanged()
    {
        var fixture = new Fixture();
        var legacy = fixture.Document();
        legacy.FileUrl = "https://example.com/old.pdf";
        await fixture.Service.AddVendorDocumentAsync(legacy);
        Assert.Equal("https://example.com/old.pdf", (await fixture.Service.GetVendorDocumentByIdAsync(legacy.Id)).FileUrl);
        await fixture.Service.DeleteVendorDocumentAsync(legacy.Id);
        Assert.Empty(fixture.Documents.Items);
    }

    private sealed class Fixture
    {
        public readonly Guid VendorId = Guid.NewGuid();
        public readonly VendorAssignment Assignment;
        public readonly MemoryRepository<VendorDocument> Documents = new(document => document.Id);
        public readonly MemoryRepository<VendorAssignment> Assignments = new(assignment => assignment.Id);
        public readonly FakeStorage Storage = new();
        public readonly FileStorageOptions Options = new();
        public readonly VendorDocumentService Service;
        public readonly byte[] Content = Encoding.UTF8.GetBytes("%PDF-1.7\nexample");

        public Fixture()
        {
            var wedding = new Wedding { CoordinatorId = "coordinator" };
            Assignment = new VendorAssignment { VendorId = VendorId, WeddingId = wedding.Id };
            Assignments.Items.Add(Assignment.Id, Assignment);
            Service = new VendorDocumentService(Documents, Assignments, new FakeWeddings(wedding), Storage,
                Microsoft.Extensions.Options.Options.Create(Options), NullLogger<VendorDocumentService>.Instance);
        }

        public VendorDocument Document() => new()
        {
            VendorId = VendorId, VendorAssignmentId = Assignment.Id, DocumentName = "Contrato"
        };

        public Task<VendorDocument> Upload() => Service.UploadAsync(
            Document(), new MemoryStream(Content), "contract.pdf", "application/pdf", "coordinator", default);
    }

    private sealed class FakeStorage : IFileStorageService
    {
        public Dictionary<string, byte[]> Objects { get; } = [];
        public bool FailUpload { get; set; }
        public bool FailDelete { get; set; }
        public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
        {
            if (FailUpload) throw new InvalidOperationException("Storage unavailable");
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            Objects.Add(key, buffer.ToArray());
        }
        public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream(Objects[key]));
        public Task DeleteAsync(string key, CancellationToken cancellationToken)
        {
            if (FailDelete) throw new InvalidOperationException("Storage unavailable");
            Objects.Remove(key);
            return Task.CompletedTask;
        }
    }

    private sealed class MemoryRepository<T>(Func<T, Guid> id) : IRepository<T> where T : class
    {
        public Dictionary<Guid, T> Items { get; } = [];
        public bool FailAdd { get; set; }
        public bool FailUpdate { get; set; }
        public Task<T> GetByIdAsync(Guid key) => Task.FromResult(Items.GetValueOrDefault(key)!);
        public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Items.Values);
        public Task<T> AddAsync(T entity)
        {
            if (FailAdd) throw new InvalidOperationException("Database unavailable");
            Items.Add(id(entity), entity);
            return Task.FromResult(entity);
        }
        public Task<T> UpdateAsync(T entity)
        {
            if (FailUpdate) throw new InvalidOperationException("Database unavailable");
            Items[id(entity)] = entity;
            return Task.FromResult(entity);
        }
        public Task DeleteAsync(Guid key) { Items.Remove(key); return Task.CompletedTask; }
        public Task<bool> ExistsAsync(Guid key) => Task.FromResult(Items.ContainsKey(key));
        public IQueryable<T> AsQueryable() => Items.Values.AsQueryable();
    }

    private sealed class FakeWeddings(Wedding wedding) : IWeddingRepository
    {
        public Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == wedding.Id ? wedding : null);
        public Task<IReadOnlyList<Wedding>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Wedding>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Wedding>?> GetByTitle(string title, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
