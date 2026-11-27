using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WeddingPlanner.Application.UseCases.VendorAssignments;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

[Route("Vendors/{vendorId:guid}/Documents")]
public sealed class VendorDocumentsController(
    GetVendorByIdHandler getVendorById,
    GetAllVendorDocumentsHandler getAllDocuments,
    GetVendorDocumentByIdHandler getDocumentById,
    AddVendorDocumentHandler addDocument,
    UpdateVendorDocumentHandler updateDocument,
    DeleteVendorDocumentHandler deleteDocument,
    StoredVendorDocumentHandler storedDocuments,
    GetAllVendorAssignmentsHandler getAllAssignments,
    GetAllWeddingsHandler getAllWeddings)
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

        var documents = new List<VendorDocument>();
        foreach (var document in (await getAllDocuments.HandleAsync()).Where(document => document.VendorId == vendorId))
        {
            if (document.StorageKey is null ||
                (document.VendorAssignmentId is Guid assignmentId &&
                 await storedDocuments.CanAccessAsync(vendorId, assignmentId, CurrentUserId, HttpContext.RequestAborted)))
                documents.Add(document);
        }

        return View(new VendorDocumentListViewModel
        {
            Vendor = vendor,
            Documents = documents
                .OrderByDescending(document => document.UploadedAt)
                .ToList(),
            Assignments = (await getAllAssignments.HandleAsync())
                .Where(assignment => assignment.VendorId == vendorId)
                .ToList()
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
    public async Task<IActionResult> Create(Guid vendorId, VendorDocumentFormViewModel model, CancellationToken cancellationToken)
    {
        if (await getVendorById.HandleAsync(new GetVendorByIdQuery(vendorId)) is null)
        {
            return NotFound();
        }

        model.Id = Guid.Empty;
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
    }

    private string? CurrentUserId =>
        User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    private async Task<VendorAssignment?> GetAssignment(Guid vendorId, Guid? assignmentId)
    {
        if (assignmentId is null)
        {
            return null;
        }

        return (await getAllAssignments.HandleAsync())
            .FirstOrDefault(assignment =>
                assignment.Id == assignmentId.Value && assignment.VendorId == vendorId);
    }

    private async Task<IActionResult> DocumentForm(VendorDocumentFormViewModel model)
    {
        ViewData["Assignments"] = (await getAllAssignments.HandleAsync())
            .Where(assignment => assignment.VendorId == model.VendorId)
            .ToList();
        ViewData["Weddings"] = (await getAllWeddings.HandleAsync()).ToDictionary(wedding => wedding.Id);
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
