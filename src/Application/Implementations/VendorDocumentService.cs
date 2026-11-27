using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class VendorDocumentService(
    IRepository<VendorDocument> repository,
    IRepository<VendorAssignment> assignments,
    IWeddingRepository weddings,
    IFileStorageService storage,
    IOptions<FileStorageOptions> options,
    ILogger<VendorDocumentService> logger) : IVendorDocumentService
{
    public async Task<IEnumerable<VendorDocument>> GetAllVendorDocumentsAsync() => await repository.GetAllAsync();
    public async Task<VendorDocument> GetVendorDocumentByIdAsync(Guid id) => await repository.GetByIdAsync(id);
    public async Task<VendorDocument> AddVendorDocumentAsync(VendorDocument document)
    {
        if (document.StorageKey is not null) throw new ValidationException("Utiliza la carga de archivos.");
        return await repository.AddAsync(document);
    }

    public async Task<VendorDocument> UpdateVendorDocumentAsync(VendorDocument document)
    {
        var existing = await repository.GetByIdAsync(document.Id);
        if (existing?.StorageKey is not null || document.StorageKey is not null)
            throw new ValidationException("No se pueden editar documentos almacenados mediante esta operación.");
        return await repository.UpdateAsync(document);
    }

    public async Task DeleteVendorDocumentAsync(Guid id)
    {
        var document = await repository.GetByIdAsync(id);
        if (document?.StorageKey is not null)
            throw new ValidationException("Utiliza la eliminación protegida del archivo.");
        await repository.DeleteAsync(id);
    }

    public async Task<bool> VendorDocumentExistsAsync(Guid id) => await repository.ExistsAsync(id);

    public async Task<bool> CanAccessStoredDocumentAsync(Guid vendorId, Guid assignmentId, string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var assignment = await assignments.GetByIdAsync(assignmentId);
        if (assignment is null || assignment.VendorId != vendorId) return false;
        var wedding = await weddings.GetByIdAsync(assignment.WeddingId, cancellationToken);
        return wedding?.CoordinatorId == userId;
    }

    public async Task<VendorDocument> UploadAsync(VendorDocument document, Stream content, string fileName, string contentType, string? userId, CancellationToken cancellationToken)
    {
        if (document.VendorAssignmentId is not Guid assignmentId ||
            !await CanAccessStoredDocumentAsync(document.VendorId, assignmentId, userId, cancellationToken))
            throw new UnauthorizedAccessException("No tienes acceso a esta contratación.");

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255 ||
            fileName.IndexOfAny(['/', '\\', ':']) >= 0 || fileName.Any(char.IsControl))
            throw new ValidationException("El nombre del archivo no es válido.");
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new ValidationException("La extensión del archivo no está permitida.");
        if (!content.CanSeek || content.Length <= 0 || settings.MaxFileSizeBytes <= 0 || content.Length > settings.MaxFileSizeBytes)
            throw new ValidationException("El archivo está vacío o supera el tamaño permitido.");

        var expectedType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => throw new ValidationException("Este formato no tiene validación de contenido compatible.")
        };
        if (!string.Equals(contentType, expectedType, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("El tipo de contenido no corresponde al formato del archivo.");
        var signature = new byte[8];
        content.Position = 0;
        var read = await content.ReadAtLeastAsync(signature, 8, throwOnEndOfStream: false, cancellationToken);
        var valid = extension switch
        {
            ".pdf" => read >= 5 && signature.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            ".png" => read >= 8 && signature.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            _ => read >= 3 && signature[0] == 0xff && signature[1] == 0xd8 && signature[2] == 0xff
        };
        if (!valid) throw new ValidationException("El contenido no coincide con el formato declarado.");
        content.Position = 0;

        document.FileUrl = null;
        document.StorageKey = Guid.NewGuid().ToString("N");
        document.FileName = fileName;
        document.ContentType = expectedType;
        document.FileSize = content.Length;
        try
        {
            await storage.UploadAsync(document.StorageKey, content, expectedType, cancellationToken);
            return await repository.AddAsync(document);
        }
        catch (Exception saveError)
        {
            try
            {
                await storage.DeleteAsync(document.StorageKey, CancellationToken.None);
            }
            catch (Exception cleanupError)
            {
                logger.LogError(cleanupError, "No se pudo limpiar el objeto {StorageKey} tras fallar la carga o el registro.", document.StorageKey);
                throw new AggregateException("No se pudo guardar el documento ni limpiar el archivo; revisa los registros.", saveError, cleanupError);
            }
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken)
    {
        var document = await GetAuthorizedStoredAsync(vendorId, documentId, userId, cancellationToken);
        if (document.DeletionPending) throw new ValidationException("El documento está pendiente de eliminación.");
        return await storage.DownloadAsync(document.StorageKey!, cancellationToken);
    }

    public async Task DeleteStoredAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken cancellationToken)
    {
        var document = await GetAuthorizedStoredAsync(vendorId, documentId, userId, cancellationToken);
        if (!document.DeletionPending)
        {
            document.DeletionPending = true;
            await repository.UpdateAsync(document);
        }
        await storage.DeleteAsync(document.StorageKey!, cancellationToken);
        await repository.DeleteAsync(document.Id);
    }

    private async Task<VendorDocument> GetAuthorizedStoredAsync(Guid vendorId, Guid id, string? userId, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id);
        if (document?.StorageKey is null || document.VendorId != vendorId ||
            document.VendorAssignmentId is not Guid assignmentId ||
            !await CanAccessStoredDocumentAsync(vendorId, assignmentId, userId, cancellationToken))
            throw new UnauthorizedAccessException("No tienes acceso a este documento.");
        return document;
    }
}
