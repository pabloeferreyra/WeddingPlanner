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
    Task<bool> CanAccessStoredDocumentAsync(Guid vendorId, Guid assignmentId, string? userId, CancellationToken cancellationToken);
    Task<VendorDocument> UploadAsync(VendorDocument document, Stream content, string fileName, string contentType, string? userId, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken);
    Task DeleteStoredAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken);
}
