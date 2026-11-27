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
using WeddingPlanner.Infrastructure.Storage;

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

    [Theory]
    [InlineData("contract.pdf", "application/pdf", "pdf")]
    [InlineData("image.png", "image/png", "png")]
    [InlineData("image.jpg", "image/jpeg", "jpeg")]
    public async Task Complete_supported_formats_are_accepted(string name, string type, string format)
    {
        var fixture = new Fixture();
        var bytes = FileBytes(format);
        var document = await fixture.Service.UploadAsync(fixture.Document(), new MemoryStream(bytes), name, type, "coordinator", default);
        Assert.Equal(bytes.Length, document.FileSize);
    }

    [Theory]
    [InlineData("contract.pdf", "application/pdf", "pdf")]
    [InlineData("image.png", "image/png", "png")]
    [InlineData("image.jpg", "image/jpeg", "jpeg")]
    public async Task Truncated_formats_are_rejected(string name, string type, string format)
    {
        var fixture = new Fixture();
        var bytes = FileBytes(format);
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
            fixture.Document(), new MemoryStream(bytes[..^8]), name, type, "coordinator", default));
    }

    [Fact]
    public async Task Empty_files_and_renamed_images_are_rejected()
    {
        var fixture = new Fixture();
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
            fixture.Document(), new MemoryStream(), "empty.pdf", "application/pdf", "coordinator", default));
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
            fixture.Document(), new MemoryStream(FileBytes("png")), "renamed.jpg", "image/jpeg", "coordinator", default));
    }

    [Fact]
    public async Task Correct_signatures_with_invalid_structure_are_rejected()
    {
        var fixture = new Fixture();
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.4\nno cross reference\n%%EOF");
        var png = FileBytes("png");
        png[16] = png[17] = png[18] = png[19] = 0;
        var jpeg = FileBytes("jpeg");
        var frame = jpeg.AsSpan().IndexOf(new byte[] { 0xff, 0xc0 });
        Assert.True(frame >= 0);
        jpeg[frame + 5] = jpeg[frame + 6] = 0;
        foreach (var (bytes, name, contentType) in new[]
        {
            (pdf, "broken.pdf", "application/pdf"),
            (png, "broken.png", "image/png"),
            (jpeg, "broken.jpg", "image/jpeg")
        })
            await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
                fixture.Document(), new MemoryStream(bytes), name, contentType, "coordinator", default));
        Assert.Empty(fixture.Storage.Objects);
    }

    private static byte[] FileBytes(string format) => format switch
    {
        "pdf" => CreatePdf(),
        "png" => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="),
        "jpeg" => Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoHBwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD8qqKKKAP/2Q=="),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    private static byte[] CreatePdf(string? embeddedContent = null)
    {
        var pdf = new StringBuilder("%PDF-1.4\n");
        var first = pdf.Length;
        pdf.Append("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        var second = pdf.Length;
        pdf.Append("2 0 obj\n<< /Type /Pages /Kids [] /Count 0 >>\nendobj\n");
        var third = pdf.Length;
        if (embeddedContent is not null)
            pdf.Append($"3 0 obj\n<< /Length {embeddedContent.Length} >>\nstream\n{embeddedContent}\nendstream\nendobj\n");
        var xref = pdf.Length;
        pdf.Append($"xref\n0 {(embeddedContent is null ? 3 : 4)}\n0000000000 65535 f \n");
        pdf.Append($"{first:D10} 00000 n \n{second:D10} 00000 n \n");
        if (embeddedContent is not null) pdf.Append($"{third:D10} 00000 n \n");
        pdf.Append($"trailer\n<< /Size {(embeddedContent is null ? 3 : 4)} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    [ClamAvIntegrationFact]
    public async Task Real_clamav_detects_eicar_and_upload_never_reaches_storage()
    {
        const string eicar = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
        var scanner = new ClamAvFileSecurityScanner(Options.Create(new ClamAvOptions
        {
            Host = "127.0.0.1",
            Port = int.Parse(Environment.GetEnvironmentVariable("CLAMAV_TEST_PORT")!)
        }), NullLogger<ClamAvFileSecurityScanner>.Instance);
        var detection = await scanner.ScanAsync(new MemoryStream(Encoding.ASCII.GetBytes(eicar)), default);
        Assert.Equal(FileScanStatus.Infected, detection.Status);
        var fixture = new Fixture(scanner);
        var clean = await fixture.Upload();
        Assert.Single(fixture.Storage.Objects);
        fixture.Storage.Objects.Clear();
        var pdf = CreatePdf(eicar);
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.UploadAsync(
            fixture.Document(), new MemoryStream(pdf), "contract.pdf", "application/pdf", "coordinator", default));
        Assert.Equal(1, fixture.Storage.UploadCalls);
        Assert.Single(fixture.Documents.Items);
        Assert.Contains(clean.Id, fixture.Documents.Items.Keys);
        Assert.Empty(fixture.Storage.Objects);
        var oldDocument = fixture.Document();
        oldDocument.StorageKey = "old-document";
        fixture.Documents.Items.Add(oldDocument.Id, oldDocument);
        fixture.Storage.Objects.Add(oldDocument.StorageKey, pdf);
        await Assert.ThrowsAsync<ValidationException>(() =>
            fixture.Service.DownloadAsync(fixture.VendorId, oldDocument.Id, "coordinator", default));
    }

    [Fact]
    public async Task File_size_limit_is_enforced()
    {
        var fixture = new Fixture();
        fixture.Options.MaxFileSizeBytes = 4;
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Upload());
        Assert.Empty(fixture.Storage.Objects);
    }

    public sealed class ClamAvIntegrationFactAttribute : FactAttribute
    {
        public ClamAvIntegrationFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CLAMAV_TEST_PORT")))
                Skip = "Set CLAMAV_TEST_PORT to run the real ClamAV integration test.";
        }
    }

    [Theory]
    [InlineData(FileScanStatus.Infected, FileScanError.None)]
    [InlineData(FileScanStatus.Error, FileScanError.Unavailable)]
    [InlineData(FileScanStatus.Error, FileScanError.Timeout)]
    [InlineData((FileScanStatus)999, FileScanError.None)]
    public async Task Unclean_or_indeterminate_scan_never_calls_storage(FileScanStatus status, FileScanError error)
    {
        var fixture = new Fixture();
        fixture.Scanner.Result = new(status, "internal-threat", error);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => fixture.Upload());
        Assert.DoesNotContain("internal-threat", exception.Message);
        Assert.Equal(1, fixture.Scanner.Calls);
        Assert.Equal(0, fixture.Storage.UploadCalls);
        Assert.Empty(fixture.Documents.Items);
    }

    [Fact]
    public async Task Contradictory_clean_result_is_rejected()
    {
        var fixture = new Fixture();
        fixture.Scanner.Result = new(FileScanStatus.Clean, Error: FileScanError.Timeout);
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Upload());
        Assert.Equal(0, fixture.Storage.UploadCalls);
    }

    [Fact]
    public async Task Scanner_failure_is_reported_without_calling_storage()
    {
        var fixture = new Fixture();
        fixture.Scanner.Fail = true;
        var error = await Assert.ThrowsAsync<ValidationException>(() => fixture.Upload());
        Assert.DoesNotContain("internal failure", error.Message);
        Assert.Equal(0, fixture.Storage.UploadCalls);
    }

    [Fact]
    public async Task Clean_scan_precedes_storage_and_both_receive_the_complete_file()
    {
        var fixture = new Fixture();
        fixture.Storage.BeforeUpload = () => Assert.Equal(1, fixture.Scanner.Calls);
        var document = await fixture.Upload();
        Assert.Equal(fixture.Content, fixture.Scanner.Scanned);
        Assert.Equal(fixture.Content, fixture.Storage.Objects[document.StorageKey!]);
    }

    [Theory]
    [InlineData(FileScanStatus.Infected)]
    [InlineData(FileScanStatus.Error)]
    public async Task Previously_stored_files_cannot_be_downloaded_without_a_clean_scan(FileScanStatus status)
    {
        var fixture = new Fixture();
        var document = fixture.Document();
        document.StorageKey = "previously-stored";
        fixture.Documents.Items.Add(document.Id, document);
        fixture.Storage.Objects.Add(document.StorageKey, fixture.Content);
        fixture.Scanner.Result = new(status);
        await Assert.ThrowsAsync<ValidationException>(() =>
            fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "coordinator", default));
        Assert.Equal(1, fixture.Scanner.Calls);
        Assert.Single(fixture.Storage.Objects);
    }

    [Fact]
    public async Task Download_scans_exact_bytes_and_disposes_temporary_file()
    {
        var fixture = new Fixture();
        var document = await fixture.Upload();
        var result = await fixture.Service.DownloadAsync(fixture.VendorId, document.Id, "coordinator", default);
        var path = Assert.IsType<FileStream>(result).Name;
        using var buffer = new MemoryStream();
        await result.CopyToAsync(buffer);
        await result.DisposeAsync();
        Assert.Equal(fixture.Content, fixture.Scanner.Scanned);
        Assert.Equal(fixture.Content, buffer.ToArray());
        Assert.False(File.Exists(path));
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
        public readonly FakeScanner Scanner = new();
        public readonly FileStorageOptions Options = new();
        public readonly VendorDocumentService Service;
        public readonly byte[] Content = CreatePdf();

        public Fixture(IFileSecurityScanner? scanner = null)
        {
            var wedding = new Wedding { CoordinatorId = "coordinator" };
            Assignment = new VendorAssignment { VendorId = VendorId, WeddingId = wedding.Id };
            Assignments.Items.Add(Assignment.Id, Assignment);
            Service = new VendorDocumentService(Documents, Assignments, new FakeWeddings(wedding), Storage, scanner ?? Scanner,
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
        public int UploadCalls { get; private set; }
        public Action? BeforeUpload { get; set; }
        public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
        {
            UploadCalls++;
            BeforeUpload?.Invoke();
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

    private sealed class FakeScanner : IFileSecurityScanner
    {
        public FileScanResult Result { get; set; } = new(FileScanStatus.Clean);
        public bool Fail { get; set; }
        public int Calls { get; private set; }
        public byte[]? Scanned { get; private set; }
        public async Task<FileScanResult> ScanAsync(Stream content, CancellationToken cancellationToken)
        {
            Calls++;
            if (Fail) throw new InvalidOperationException("internal failure");
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            Scanned = buffer.ToArray();
            return Result;
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
