using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;

public interface IVendorPaymentRepository
{
    Task<IReadOnlyList<VendorPayment>> GetByWeddingAsync(Guid weddingId);
    Task<decimal> GetTotalCommittedByAssignmentAsync(Guid assignmentId, Guid? excludedPaymentId = null);
}
