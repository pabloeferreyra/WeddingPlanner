using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class VendorDocumentQueries(ApplicationDbContext context) : IVendorDocumentQueries
{
    public async Task<IReadOnlyList<VendorDocument>> GetVisibleByVendorAsync(Guid vendorId, string? userId, CancellationToken cancellationToken) =>
        await VisibleByVendorQuery(vendorId, userId).ToListAsync(cancellationToken);

    internal IQueryable<VendorDocument> VisibleByVendorQuery(Guid vendorId, string? userId) =>
        context.Set<VendorDocument>().AsNoTracking()
            .Where(document => document.VendorId == vendorId &&
                (document.StorageKey == null ||
                 (!string.IsNullOrWhiteSpace(userId) && document.VendorAssignment != null &&
                  document.VendorAssignment.VendorId == vendorId &&
                  document.VendorAssignment.Wedding != null &&
                  document.VendorAssignment.Wedding.CoordinatorId == userId)))
            .OrderByDescending(document => document.UploadedAt);

    public async Task<IReadOnlyList<VendorDocumentAssignment>> GetAssignmentsByVendorAsync(Guid vendorId, CancellationToken cancellationToken) =>
        await context.Set<VendorAssignment>().AsNoTracking()
            .Where(assignment => assignment.VendorId == vendorId)
            .Select(assignment => new VendorDocumentAssignment(assignment.Id, assignment.WeddingId,
                assignment.Wedding != null ? assignment.Wedding.Title : null,
                assignment.ContractDate, assignment.PaymentAmount))
            .ToListAsync(cancellationToken);

    public Task<bool> HasDocumentsForVendorAsync(Guid vendorId, CancellationToken cancellationToken) =>
        context.Set<VendorDocument>().AsNoTracking().AnyAsync(document => document.VendorId == vendorId, cancellationToken);

    public Task<bool> HasDocumentsForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken) =>
        context.Set<VendorDocument>().AsNoTracking().AnyAsync(document => document.VendorAssignmentId == assignmentId, cancellationToken);
}
