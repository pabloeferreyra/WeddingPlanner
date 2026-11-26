using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IVendorPaymentService
{
    Task<IReadOnlyList<VendorPayment>> GetByWeddingAsync(Guid weddingId);
    Task<VendorPayment> GetByIdAsync(Guid paymentId);
    Task<VendorPayment> AddAsync(VendorPayment payment);
    Task<VendorPayment> UpdateAsync(VendorPayment payment);
    Task DeleteAsync(Guid paymentId);
}
