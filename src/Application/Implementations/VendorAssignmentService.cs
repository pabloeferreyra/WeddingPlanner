using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorAssignmentService(
    IRepository<VendorAssignment> repository,
    IVendorPaymentRepository paymentRepository) : IVendorAssignmentService
{
    public async Task<IEnumerable<VendorAssignment>> GetAllVendorAssignmentsAsync() => await repository.GetAllAsync();
    public async Task<VendorAssignment> GetVendorAssignmentByIdAsync(Guid vendorAssignmentId) => await repository.GetByIdAsync(vendorAssignmentId);

    public async Task<VendorAssignment> AddVendorAssignmentAsync(VendorAssignment vendorAssignment)
    {
        ValidateContract(vendorAssignment);
        if (vendorAssignment.OpeningBalancePaid > vendorAssignment.PaymentAmount)
        {
            throw new ValidationException("El importe pagado previamente no puede superar el importe contratado.");
        }

        return await repository.AddAsync(vendorAssignment);
    }

    public async Task<VendorAssignment> UpdateVendorAssignmentAsync(VendorAssignment vendorAssignment)
    {
        ValidateContract(vendorAssignment);
        var current = await repository.GetByIdAsync(vendorAssignment.Id);
        if (current is null)
        {
            throw new ValidationException("No se encontró la contratación del proveedor.");
        }

        var committed = current.OpeningBalancePaid
            + await paymentRepository.GetTotalCommittedByAssignmentAsync(current.Id);
        if (vendorAssignment.PaymentAmount < committed
            && vendorAssignment.PaymentAmount != current.PaymentAmount)
        {
            throw new ValidationException(
                "El total contratado no puede ser menor que las cuotas y pagos previos registrados.");
        }

        return await repository.UpdateAsync(vendorAssignment);
    }

    public async Task DeleteVendorAssignmentAsync(Guid vendorAssignmentId) => await repository.DeleteAsync(vendorAssignmentId);
    public async Task<bool> VendorAssignmentExistsAsync(Guid vendorAssignmentId) => await repository.ExistsAsync(vendorAssignmentId);

    private static void ValidateContract(VendorAssignment assignment)
    {
        if (assignment.PaymentAmount < 0 || assignment.OpeningBalancePaid < 0)
        {
            throw new ValidationException("Los importes de la contratación no pueden ser negativos.");
        }

        if (assignment.DepositPercentage is < 0 or > 100)
        {
            throw new ValidationException("El porcentaje de anticipo debe estar entre 0 y 100.");
        }
    }
}
