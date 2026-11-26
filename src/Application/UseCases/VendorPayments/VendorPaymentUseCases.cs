using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.VendorPayments;

public sealed record GetVendorPaymentsByWeddingQuery(Guid WeddingId);

public sealed class GetVendorPaymentsByWeddingHandler(IVendorPaymentService service)
{
    public Task<IReadOnlyList<VendorPayment>> HandleAsync(GetVendorPaymentsByWeddingQuery query) =>
        service.GetByWeddingAsync(query.WeddingId);
}

public sealed record GetVendorPaymentByIdQuery(Guid PaymentId);

public sealed class GetVendorPaymentByIdHandler(IVendorPaymentService service)
{
    public Task<VendorPayment> HandleAsync(GetVendorPaymentByIdQuery query) =>
        service.GetByIdAsync(query.PaymentId);
}

public sealed record AddVendorPaymentCommand(VendorPayment Payment);

public sealed class AddVendorPaymentHandler(IVendorPaymentService service)
{
    public Task<VendorPayment> HandleAsync(AddVendorPaymentCommand command) =>
        service.AddAsync(command.Payment);
}

public sealed record UpdateVendorPaymentCommand(VendorPayment Payment);

public sealed class UpdateVendorPaymentHandler(IVendorPaymentService service)
{
    public Task<VendorPayment> HandleAsync(UpdateVendorPaymentCommand command) =>
        service.UpdateAsync(command.Payment);
}

public sealed record DeleteVendorPaymentCommand(Guid PaymentId);

public sealed class DeleteVendorPaymentHandler(IVendorPaymentService service)
{
    public Task HandleAsync(DeleteVendorPaymentCommand command) =>
        service.DeleteAsync(command.PaymentId);
}
