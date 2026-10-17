using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class AddressService(IRepository<Address> repository) : IAddressService
{
    public async Task<IEnumerable<Address>> GetAllAddressesAsync() => await repository.GetAllAsync();
    public async Task<Address> GetAddressByIdAsync(Guid addressId) => await repository.GetByIdAsync(addressId);
    public async Task<Address> AddAddressAsync(Address address) => await repository.AddAsync(address);
    public async Task<Address> UpdateAddressAsync(Address address) => await repository.UpdateAsync(address);
    public async Task DeleteAddressAsync(Guid addressId) => await repository.DeleteAsync(addressId);
    public async Task<bool> AddressExistsAsync(Guid addressId) => await repository.ExistsAsync(addressId);
}
