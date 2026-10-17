using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IAddressService
{
    Task<IEnumerable<Address>> GetAllAddressesAsync();
    Task<Address> GetAddressByIdAsync(Guid addressId);
    Task<Address> AddAddressAsync(Address address);
    Task<Address> UpdateAddressAsync(Address address);
    Task DeleteAddressAsync(Guid addressId);
    Task<bool> AddressExistsAsync(Guid addressId);
}
