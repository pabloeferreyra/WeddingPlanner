using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.VendorDocuments;

public sealed class GetAllVendorDocumentsHandler(IVendorDocumentService service)
{
    public Task<IEnumerable<VendorDocument>> HandleAsync() => service.GetAllVendorDocumentsAsync();
}

public sealed record GetVendorDocumentByIdQuery(Guid VendorDocumentId);

public sealed class GetVendorDocumentByIdHandler(IVendorDocumentService service)
{
    public Task<VendorDocument> HandleAsync(GetVendorDocumentByIdQuery query) =>
        service.GetVendorDocumentByIdAsync(query.VendorDocumentId);
}

public sealed record AddVendorDocumentCommand(VendorDocument VendorDocument);

public sealed class AddVendorDocumentHandler(IVendorDocumentService service)
{
    public Task<VendorDocument> HandleAsync(AddVendorDocumentCommand command) =>
        service.AddVendorDocumentAsync(command.VendorDocument);
}

public sealed record UpdateVendorDocumentCommand(VendorDocument VendorDocument);

public sealed class UpdateVendorDocumentHandler(IVendorDocumentService service)
{
    public Task<VendorDocument> HandleAsync(UpdateVendorDocumentCommand command) =>
        service.UpdateVendorDocumentAsync(command.VendorDocument);
}

public sealed record DeleteVendorDocumentCommand(Guid VendorDocumentId);

public sealed class DeleteVendorDocumentHandler(IVendorDocumentService service)
{
    public Task HandleAsync(DeleteVendorDocumentCommand command) =>
        service.DeleteVendorDocumentAsync(command.VendorDocumentId);
}

public sealed record VendorDocumentExistsQuery(Guid VendorDocumentId);

public sealed class VendorDocumentExistsHandler(IVendorDocumentService service)
{
    public Task<bool> HandleAsync(VendorDocumentExistsQuery query) =>
        service.VendorDocumentExistsAsync(query.VendorDocumentId);
}

public sealed class StoredVendorDocumentHandler(IVendorDocumentService service)
{
    public Task<bool> CanAccessAsync(Guid vendorId, Guid assignmentId, string? userId, CancellationToken cancellationToken) =>
        service.CanAccessStoredDocumentAsync(vendorId, assignmentId, userId, cancellationToken);

    public Task<VendorDocument> UploadAsync(VendorDocument document, Stream content, string fileName, string contentType, string? userId, CancellationToken cancellationToken) =>
        service.UploadAsync(document, content, fileName, contentType, userId, cancellationToken);

    public Task<Stream> DownloadAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken) =>
        service.DownloadAsync(vendorId, documentId, userId, cancellationToken);

    public Task DeleteAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken) =>
        service.DeleteStoredAsync(vendorId, documentId, userId, cancellationToken);
}
