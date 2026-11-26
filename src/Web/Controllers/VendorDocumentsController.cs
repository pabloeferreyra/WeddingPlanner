using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.VendorAssignments;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
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
    GetAllVendorAssignmentsHandler getAllAssignments)
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
            Documents = (await getAllDocuments.HandleAsync())
                .Where(document => document.VendorId == vendorId)
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
        if (document is null || document.VendorId != vendorId)
        {
            return NotFound();
        }

        return await DocumentForm(ToFormModel(document));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid vendorId, VendorDocumentFormViewModel model)
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
        ValidateFileUrl(model);

        if (!ModelState.IsValid)
        {
            return await DocumentForm(model);
        }

        await addDocument.HandleAsync(new AddVendorDocumentCommand(ToEntity(model)));
        return RedirectToAction(nameof(Index), new { vendorId });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid vendorId, Guid id, VendorDocumentFormViewModel model)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId)
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
    public async Task<IActionResult> Delete(Guid vendorId, Guid id)
    {
        var document = await getDocumentById.HandleAsync(new GetVendorDocumentByIdQuery(id));
        if (document is null || document.VendorId != vendorId)
        {
            return NotFound();
        }

        await deleteDocument.HandleAsync(new DeleteVendorDocumentCommand(id));
        return RedirectToAction(nameof(Index), new { vendorId });
    }

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
        ExpiryDate = model.ExpiryDate
    };
}
