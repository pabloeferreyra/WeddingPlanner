using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Vendors;

public sealed class GetAllVendorsHandler(IVendorService service)
{
    public Task<IEnumerable<Vendor>> HandleAsync() => service.GetAllVendorsAsync();
}

public sealed record GetVendorByIdQuery(Guid VendorId);

public sealed class GetVendorByIdHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(GetVendorByIdQuery query) => service.GetVendorByIdAsync(query.VendorId);
}

public sealed record AddVendorCommand(Vendor Vendor);

public sealed class AddVendorHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(AddVendorCommand command) => service.AddVendorAsync(command.Vendor);
}

public sealed record UpdateVendorCommand(Vendor Vendor);

public sealed class UpdateVendorHandler(IVendorService service)
{
    public Task<Vendor> HandleAsync(UpdateVendorCommand command) => service.UpdateVendorAsync(command.Vendor);
}

public sealed record DeleteVendorCommand(Guid VendorId);

public sealed class DeleteVendorHandler(IVendorService service)
{
    public Task HandleAsync(DeleteVendorCommand command) => service.DeleteVendorAsync(command.VendorId);
}

public sealed record VendorExistsQuery(Guid VendorId);

public sealed class VendorExistsHandler(IVendorService service)
{
    public Task<bool> HandleAsync(VendorExistsQuery query) => service.VendorExistsAsync(query.VendorId);
}
