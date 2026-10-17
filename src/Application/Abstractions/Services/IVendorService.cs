using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IVendorService
{
    Task<IEnumerable<Vendor>> GetAllVendorsAsync();
    Task<Vendor> GetVendorByIdAsync(Guid vendorId);
    Task<Vendor> AddVendorAsync(Vendor vendor);
    Task<Vendor> UpdateVendorAsync(Vendor vendor);
    Task DeleteVendorAsync(Guid vendorId);
    Task<bool> VendorExistsAsync(Guid vendorId);
}
