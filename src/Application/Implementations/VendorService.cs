using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorService(IRepository<Vendor> repository) : IVendorService
{
    public async Task<IEnumerable<Vendor>> GetAllVendorsAsync() => await repository.GetAllAsync();

    public async Task<Vendor> GetVendorByIdAsync(Guid vendorId) => await repository.GetByIdAsync(vendorId);

    public async Task<Vendor> AddVendorAsync(Vendor vendor) => await repository.AddAsync(vendor);

    public async Task<Vendor> UpdateVendorAsync(Vendor vendor) => await repository.UpdateAsync(vendor);

    public async Task DeleteVendorAsync(Guid vendorId) => await repository.DeleteAsync(vendorId);

    public async Task<bool> VendorExistsAsync(Guid vendorId) => await repository.ExistsAsync(vendorId);
}
