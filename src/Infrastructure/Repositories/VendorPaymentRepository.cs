using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class VendorPaymentRepository(ApplicationDbContext context) : IVendorPaymentRepository
{
    public async Task<IReadOnlyList<VendorPayment>> GetByWeddingAsync(Guid weddingId) =>
        await context.VendorPayments
            .Where(payment => payment.VendorAssignment!.WeddingId == weddingId)
            .OrderByDescending(payment => payment.PaymentDate ?? payment.DueDate ?? payment.CreatedAt)
            .ToListAsync();

    public async Task<decimal> GetTotalCommittedByAssignmentAsync(
        Guid assignmentId,
        Guid? excludedPaymentId = null)
    {
        var payments = context.VendorPayments
            .Where(payment => payment.VendorAssignmentId == assignmentId);

        if (excludedPaymentId.HasValue)
        {
            payments = payments.Where(payment => payment.Id != excludedPaymentId.Value);
        }

        return await payments.SumAsync(payment => (decimal?)payment.Amount) ?? 0m;
    }
}
