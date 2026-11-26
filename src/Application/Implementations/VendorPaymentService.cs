using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorPaymentService(
    IRepository<VendorPayment> paymentRepository,
    IRepository<VendorAssignment> assignmentRepository,
    IVendorPaymentRepository vendorPaymentRepository) : IVendorPaymentService
{
    public Task<IReadOnlyList<VendorPayment>> GetByWeddingAsync(Guid weddingId) =>
        vendorPaymentRepository.GetByWeddingAsync(weddingId);

    public Task<VendorPayment> GetByIdAsync(Guid paymentId) =>
        paymentRepository.GetByIdAsync(paymentId);

    public async Task<VendorPayment> AddAsync(VendorPayment payment)
    {
        ValidatePayment(payment);
        var assignment = await GetAssignment(payment.VendorAssignmentId);
        await EnsureWithinContract(payment, assignment);
        return await paymentRepository.AddAsync(payment);
    }

    public async Task<VendorPayment> UpdateAsync(VendorPayment payment)
    {
        ValidatePayment(payment);
        var current = await paymentRepository.GetByIdAsync(payment.Id);
        if (current is null || current.VendorAssignmentId != payment.VendorAssignmentId)
        {
            throw new ValidationException("El pago no pertenece a esta contratación.");
        }

        var assignment = await GetAssignment(payment.VendorAssignmentId);
        await EnsureWithinContract(payment, assignment, payment.Id);
        return await paymentRepository.UpdateAsync(payment);
    }

    public async Task DeleteAsync(Guid paymentId)
    {
        await paymentRepository.DeleteAsync(paymentId);
    }

    private async Task<VendorAssignment> GetAssignment(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId);
        return assignment ?? throw new ValidationException("No se encontró la contratación del proveedor.");
    }

    private async Task EnsureWithinContract(
        VendorPayment payment,
        VendorAssignment assignment,
        Guid? excludedPaymentId = null)
    {
        var committed = assignment.OpeningBalancePaid
            + await vendorPaymentRepository.GetTotalCommittedByAssignmentAsync(
                assignment.Id,
                excludedPaymentId);
        if (committed + payment.Amount > assignment.PaymentAmount)
        {
            throw new ValidationException(
                "El importe de las cuotas no puede superar el total contratado.");
        }
    }

    private static void ValidatePayment(VendorPayment payment)
    {
        if (payment.Amount <= 0)
        {
            throw new ValidationException("El importe del pago debe ser mayor que cero.");
        }

        if (!Enum.IsDefined(payment.Type))
        {
            throw new ValidationException("Selecciona un tipo de pago válido.");
        }

        if ((payment.DueDate.HasValue && payment.DueDate.Value == default)
            || (payment.PaymentDate.HasValue && payment.PaymentDate.Value == default))
        {
            throw new ValidationException("Las fechas del pago no son válidas.");
        }

        if (payment.PaymentMethod?.Length > 100)
        {
            throw new ValidationException("El medio de pago no puede superar los 100 caracteres.");
        }

        if (payment.Notes?.Length > 500)
        {
            throw new ValidationException("Las notas no pueden superar los 500 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(payment.ReceiptUrl)
            && (!Uri.TryCreate(payment.ReceiptUrl, UriKind.Absolute, out var receiptUri)
                || (receiptUri.Scheme != Uri.UriSchemeHttps && receiptUri.Scheme != Uri.UriSchemeHttp)))
        {
            throw new ValidationException("El comprobante debe tener un enlace http:// o https:// válido.");
        }
    }
}
