using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

[Route("Vendors/{vendorId:guid}/Documents")]
public sealed class VendorDocumentsController(
    GetVendorByIdHandler getVendorById,
    VendorDocumentQueriesHandler documentQueries,
    GetVendorDocumentByIdHandler getDocumentById,
    AddVendorDocumentHandler addDocument,
    UpdateVendorDocumentHandler updateDocument,
    DeleteVendorDocumentHandler deleteDocument,
    StoredVendorDocumentHandler storedDocuments)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid vendorId)
    {
        var vendor = await getVendorById.HandleAsync(new GetVendorByIdQuery(vendorId));
        if (vendor is null)
        {
            return NotFound();
        }

        return View(new VendorDocumentListViewModel
        {
            Vendor = vendor,
            Documents = await documentQueries.GetVisibleAsync(vendorId, CurrentUserId, HttpContext.RequestAborted),
            Assignments = await documentQueries.GetAssignmentsAsync(vendorId, HttpContext.RequestAborted)
        });
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(Guid vendorId)
    {
        if (await getVendorById.HandleAsync(new GetVendorByIdQuery(vendorId)) is null)
        {
            return NotFound();
        }

        return await DocumentForm(new VendorDocumentFormViewModel { VendorId = vendorId });
    }

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid vendorId, Guid id)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId || document.StorageKey is not null)
        {
            return NotFound();
        }

        return await DocumentForm(ToFormModel(document));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(FileStorageOptions.MaximumRequestSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileStorageOptions.MaximumFileSizeBytes)]
    public async Task<IActionResult> Create(Guid vendorId, VendorDocumentFormViewModel? model, CancellationToken cancellationToken)
    {
        if (Request.ContentLength > FileStorageOptions.MaximumRequestSizeBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);
        if (model is null)
            return BadRequest("No se pudo procesar el formulario. Comprueba que el archivo no supere 10 MiB.");

        if (await getVendorById.HandleAsync(new GetVendorByIdQuery(vendorId)) is null)
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
        model.VendorId = vendorId;
        if (Request.HasFormContentType && Request.Form.Files.Count > 1)
            ModelState.AddModelError(nameof(model.File), "Solo se permite un archivo por solicitud.");
        var assignment = await GetAssignment(vendorId, model.VendorAssignmentId);
        if (model.VendorAssignmentId is not null && assignment is null)
        {
            ModelState.AddModelError(nameof(model.VendorAssignmentId), "La asignación no pertenece a este proveedor.");
        }
        if (!Enum.IsDefined(model.DocumentType))
        {
            ModelState.AddModelError(nameof(model.DocumentType), "Selecciona un tipo de documento válido.");
        }
        if (model.File is not null && !string.IsNullOrWhiteSpace(model.FileUrl))
            ModelState.AddModelError(nameof(model.File), "Elige un archivo o un enlace, no ambos.");
        else if (model.File is null)
            ValidateFileUrl(model);
        else if (model.VendorAssignmentId is null)
            ModelState.AddModelError(nameof(model.VendorAssignmentId), "Selecciona la contratación de la boda.");

        if (!ModelState.IsValid)
        {
            return await DocumentForm(model);
        }

        if (model.File is null)
        {
            await addDocument.HandleAsync(new AddVendorDocumentCommand(ToEntity(model)));
        }
        else
        {
            try
            {
                await using var stream = model.File.OpenReadStream();
                await storedDocuments.UploadAsync(ToEntity(model), stream, model.File.FileName, model.File.ContentType,
                    CurrentUserId, cancellationToken);
            }
            catch (ValidationException exception)
            {
                ModelState.AddModelError(nameof(model.File), exception.Message);
                return await DocumentForm(model);
            }
            catch (UnauthorizedAccessException)
            {
                ModelState.AddModelError(nameof(model.VendorAssignmentId), "Inicia sesión como coordinador de la boda de esta contratación.");
                return await DocumentForm(model);
            }
        }
        return RedirectToAction(nameof(Index), new { vendorId });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid vendorId, Guid id, VendorDocumentFormViewModel model)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId || document.StorageKey is not null)
        {
            return NotFound();
        }

        model.Id = id;
        model.VendorId = vendorId;
        var assignment = await GetAssignment(vendorId, model.VendorAssignmentId);
        if (model.VendorAssignmentId is not null && assignment is null)
        {
            ModelState.AddModelError(nameof(model.VendorAssignmentId), "La asignación no pertenece a este proveedor.");
        }
        if (!Enum.IsDefined(model.DocumentType))
        {
            ModelState.AddModelError(nameof(model.DocumentType), "Selecciona un tipo de documento válido.");
        }
        ValidateFileUrl(model);

        if (!ModelState.IsValid)
        {
            return await DocumentForm(model);
        }

        document.VendorAssignmentId = model.VendorAssignmentId;
        document.DocumentName = model.DocumentName;
        document.DocumentType = model.DocumentType;
        document.FileUrl = model.FileUrl;
        document.ExpiryDate = model.ExpiryDate;
        await updateDocument.HandleAsync(new UpdateVendorDocumentCommand(document));
        return RedirectToAction(nameof(Index), new { vendorId });
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid vendorId, Guid id, CancellationToken cancellationToken)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId)
        {
            return NotFound();
        }

        if (document.StorageKey is null)
            await deleteDocument.HandleAsync(new DeleteVendorDocumentCommand(id));
        else
        {
            try
            {
                await storedDocuments.DeleteAsync(vendorId, id, CurrentUserId, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
        }
        return RedirectToAction(nameof(Index), new { vendorId });
    }

    [HttpGet("{id:guid}/Download")]
    public async Task<IActionResult> Download(Guid vendorId, Guid id, CancellationToken cancellationToken)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId || document.StorageKey is null || document.DeletionPending)
            return NotFound();
        try
        {
            var stream = await storedDocuments.DownloadAsync(vendorId, id, CurrentUserId, cancellationToken);
            Response.Headers.XContentTypeOptions = "nosniff";
            Response.Headers.CacheControl = "private, no-store";
            return File(stream, document.ContentType!, document.FileName!);
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound();
        }
        catch (ValidationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "No se pudo verificar el documento. Inténtalo nuevamente.");
        }
    }

    private string? CurrentUserId =>
        User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    private async Task<VendorDocumentAssignment?> GetAssignment(Guid vendorId, Guid? assignmentId)
    {
        if (assignmentId is null)
        {
            return null;
        }

        return (await documentQueries.GetAssignmentsAsync(vendorId, HttpContext.RequestAborted))
            .FirstOrDefault(assignment => assignment.Id == assignmentId.Value);
    }

    private async Task<IActionResult> DocumentForm(VendorDocumentFormViewModel model)
    {
        ViewData["Assignments"] = await documentQueries.GetAssignmentsAsync(model.VendorId, HttpContext.RequestAborted);
        return PartialView("Forms/_DocumentForm", model);
    }

    private void ValidateFileUrl(VendorDocumentFormViewModel model)
    {
        if (!Uri.TryCreate(model.FileUrl, UriKind.Absolute, out var fileUri)
            || (fileUri.Scheme != Uri.UriSchemeHttps && fileUri.Scheme != Uri.UriSchemeHttp))
        {
            ModelState.AddModelError(nameof(model.FileUrl), "El enlace debe comenzar con http:// o https://.");
        }
    }

    private static VendorDocumentFormViewModel ToFormModel(VendorDocument document) => new()
    {
        Id = document.Id,
        VendorId = document.VendorId,
        VendorAssignmentId = document.VendorAssignmentId,
        DocumentName = document.DocumentName ?? string.Empty,
        DocumentType = document.DocumentType,
        FileUrl = document.FileUrl ?? string.Empty,
        ExpiryDate = document.ExpiryDate
    };

    private static VendorDocument ToEntity(VendorDocumentFormViewModel model) => new()
    {
        VendorId = model.VendorId,
        VendorAssignmentId = model.VendorAssignmentId,
        DocumentName = model.DocumentName,
        DocumentType = model.DocumentType,
        FileUrl = model.FileUrl,
        Description = model.Description,
        ExpiryDate = model.ExpiryDate
    };
}
