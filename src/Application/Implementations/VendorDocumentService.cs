using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorDocumentService(IRepository<VendorDocument> repository) : IVendorDocumentService
{
    public async Task<IEnumerable<VendorDocument>> GetAllVendorDocumentsAsync() => await repository.GetAllAsync();
    public async Task<VendorDocument> GetVendorDocumentByIdAsync(Guid vendorDocumentId) => await repository.GetByIdAsync(vendorDocumentId);
    public async Task<VendorDocument> AddVendorDocumentAsync(VendorDocument vendorDocument) => await repository.AddAsync(vendorDocument);
    public async Task<VendorDocument> UpdateVendorDocumentAsync(VendorDocument vendorDocument) => await repository.UpdateAsync(vendorDocument);
    public async Task DeleteVendorDocumentAsync(Guid vendorDocumentId) => await repository.DeleteAsync(vendorDocumentId);
    public async Task<bool> VendorDocumentExistsAsync(Guid vendorDocumentId) => await repository.ExistsAsync(vendorDocumentId);
}
