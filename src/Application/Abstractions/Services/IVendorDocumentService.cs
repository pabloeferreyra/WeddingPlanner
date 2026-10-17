using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IVendorDocumentService
{
    Task<IEnumerable<VendorDocument>> GetAllVendorDocumentsAsync();
    Task<VendorDocument> GetVendorDocumentByIdAsync(Guid vendorDocumentId);
    Task<VendorDocument> AddVendorDocumentAsync(VendorDocument vendorDocument);
    Task<VendorDocument> UpdateVendorDocumentAsync(VendorDocument vendorDocument);
    Task DeleteVendorDocumentAsync(Guid vendorDocumentId);
    Task<bool> VendorDocumentExistsAsync(Guid vendorDocumentId);
}
