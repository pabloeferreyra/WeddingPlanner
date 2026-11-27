using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;
using WeddingPlanner.Infrastructure.Repositories;

namespace WeddingPlanner.Tests.Unit;

public class VendorDocumentQueriesTests
{
    [Fact]
    public async Task Queries_filter_vendor_and_owner_and_preserve_legacy_documents()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new ApplicationDbContext(options);
        var vendorId = Guid.NewGuid();
        var otherVendor = Guid.NewGuid();
        var wedding = new Wedding { Title = "First", CoordinatorId = "owner" };
        var otherWedding = new Wedding { Title = "Second", CoordinatorId = "other" };
        var first = new VendorAssignment { VendorId = vendorId, Wedding = wedding };
        var second = new VendorAssignment { VendorId = vendorId, Wedding = otherWedding };
        var third = new VendorAssignment { VendorId = otherVendor, Wedding = wedding };
        context.AddRange(first, second, third);
        var legacy = new VendorDocument { VendorId = vendorId, DocumentName = "Legacy", FileUrl = "https://example.invalid" };
        var own = new VendorDocument { VendorId = vendorId, VendorAssignment = first, DocumentName = "Own", StorageKey = "own" };
        var foreignWedding = new VendorDocument { VendorId = vendorId, VendorAssignment = second, DocumentName = "Other wedding", StorageKey = "other" };
        var foreignVendor = new VendorDocument { VendorId = otherVendor, VendorAssignment = third, DocumentName = "Other vendor", StorageKey = "vendor" };
        var inconsistent = new VendorDocument { VendorId = vendorId, VendorAssignment = third, DocumentName = "Mismatched", StorageKey = "mismatched" };
        context.AddRange(legacy, own, foreignWedding, foreignVendor, inconsistent);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var queries = new VendorDocumentQueries(context);

        var visible = await queries.GetVisibleByVendorAsync(vendorId, "owner", default);
        Assert.Equal(2, visible.Count);
        Assert.Contains(visible, document => document.Id == legacy.Id);
        Assert.Contains(visible, document => document.Id == own.Id);
        Assert.Single(await queries.GetVisibleByVendorAsync(vendorId, null, default));
        Assert.Single(await queries.GetVisibleByVendorAsync(vendorId, " ", default));
        Assert.Empty(await queries.GetVisibleByVendorAsync(Guid.NewGuid(), "owner", default));
        var assignments = await queries.GetAssignmentsByVendorAsync(vendorId, default);
        Assert.Equal(2, assignments.Count);
        Assert.Contains(assignments, assignment => assignment.Id == first.Id && assignment.WeddingTitle == "First");
        Assert.True(await queries.HasDocumentsForVendorAsync(vendorId, default));
        Assert.False(await queries.HasDocumentsForVendorAsync(Guid.NewGuid(), default));
        Assert.True(await queries.HasDocumentsForAssignmentAsync(second.Id, default));
        Assert.False(await queries.HasDocumentsForAssignmentAsync(Guid.NewGuid(), default));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public void Visible_documents_query_is_filtered_and_joined_in_postgresql()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sql_translation").Options;
        using var context = new ApplicationDbContext(options);
        var sql = new VendorDocumentQueries(context).VisibleByVendorQuery(Guid.NewGuid(), "owner").ToQueryString();
        Assert.Contains("WHERE", sql);
        Assert.Contains("\"VendorId\"", sql);
        Assert.Contains("\"CoordinatorId\"", sql);
        Assert.Contains("JOIN", sql);
        Assert.Contains("\"StorageKey\"", sql);
    }
}
