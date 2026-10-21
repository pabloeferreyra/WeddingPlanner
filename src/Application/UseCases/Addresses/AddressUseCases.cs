using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Addresses;

public sealed class GetAllAddressesHandler(IAddressService service)
{
    public Task<IEnumerable<Address>> HandleAsync() => service.GetAllAddressesAsync();
}

public sealed record GetAddressByIdQuery(Guid AddressId);

public sealed class GetAddressByIdHandler(IAddressService service)
{
    public Task<Address> HandleAsync(GetAddressByIdQuery query) => service.GetAddressByIdAsync(query.AddressId);
}

public sealed record AddAddressCommand(Address Address);

public sealed class AddAddressHandler(IAddressService service)
{
    public Task<Address> HandleAsync(AddAddressCommand command) => service.AddAddressAsync(command.Address);
}

public sealed record UpdateAddressCommand(Address Address);

public sealed class UpdateAddressHandler(IAddressService service)
{
    public Task<Address> HandleAsync(UpdateAddressCommand command) => service.UpdateAddressAsync(command.Address);
}

public sealed record DeleteAddressCommand(Guid AddressId);

public sealed class DeleteAddressHandler(IAddressService service)
{
    public Task HandleAsync(DeleteAddressCommand command) => service.DeleteAddressAsync(command.AddressId);
}

public sealed record AddressExistsQuery(Guid AddressId);

public sealed class AddressExistsHandler(IAddressService service)
{
    public Task<bool> HandleAsync(AddressExistsQuery query) => service.AddressExistsAsync(query.AddressId);
}
