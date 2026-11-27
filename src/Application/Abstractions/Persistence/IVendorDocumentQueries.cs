using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;

public sealed record VendorDocumentAssignment(Guid Id, Guid WeddingId, string? WeddingTitle, DateTime ContractDate, decimal PaymentAmount);

public interface IVendorDocumentQueries
{
    Task<IReadOnlyList<VendorDocument>> GetVisibleByVendorAsync(Guid vendorId, string? userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<VendorDocumentAssignment>> GetAssignmentsByVendorAsync(Guid vendorId, CancellationToken cancellationToken);
    Task<bool> HasDocumentsForVendorAsync(Guid vendorId, CancellationToken cancellationToken);
    Task<bool> HasDocumentsForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken);
}
