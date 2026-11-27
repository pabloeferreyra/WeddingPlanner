using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Web.Helpers;
using WeddingPlanner.Web.Models;
using WeddingPlanner.Core.Domain.Entities;
namespace WeddingPlanner.Web.Controllers;

[Route("Vendors")]
public class VendorsController(
    GetAllVendorsHandler getAllVendorsHandler,
    AddVendorHandler addVendorHandler,
    GetVendorByIdHandler getVendorByIdHandler,
    UpdateVendorHandler updateVendorHandler,
    DeleteVendorHandler deleteVendorHandler,
    GetAllVendorDocumentsHandler getAllVendorDocuments
) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpPost("InitializeTable")]
    public async Task<IActionResult> InitializeTable(CancellationToken cancellationToken)
    {
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);
        var vendors = await getAllVendorsHandler.HandleAsync();
        var filteredVendors = vendors.Skip(skip).Take(pageSize).ToList();
        var response = new
        {
            draw,
            recordsTotal = vendors.Count(),
            recordsFiltered = vendors.Count(),
            data = filteredVendors
        };
        return Json(response);
    }

    [HttpGet("Create")]
    public IActionResult Create() => PartialView("_Create", new VendorFormViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendorFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_Create", model);
        }

        await addVendorHandler.HandleAsync(new AddVendorCommand
        {
            BusinessName = model.BusinessName,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            PhoneAreaCode = model.Phone?.AreaCode,
            PhoneNumber = model.Phone?.Number,
            Website = model.Website,
            ServiceType = model.ServiceType,
            Street = model.Address.Street,
            City = model.Address.City,
            State = model.Address.State,
            ZipCode = model.Address.ZipCode,
            Description = model.Description,
            AverageRating = model.AverageRating,
            IsActive = model.IsActive,
            IsFavorite = model.IsFavorite,
            FacebookUrl = model.FacebookUrl,
            InstagramUrl = model.InstagramUrl
        });

        return RedirectToAction("Index");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var selectedVendor = await getVendorByIdHandler.HandleAsync(new GetVendorByIdQuery(id));
        if (selectedVendor is null)
        {
            return NotFound();
        }
        return PartialView("_Details", selectedVendor);
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var selectedVendor = await getVendorByIdHandler.HandleAsync(new GetVendorByIdQuery(id));
       return selectedVendor is null ? NotFound() : PartialView("_Edit", MapToViewModel(selectedVendor));
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, VendorFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_Edit", model);
        }

        var selectedVendor = await getVendorByIdHandler.HandleAsync(new GetVendorByIdQuery(id));
        if (selectedVendor is null)
        {
            return NotFound();
        }

        selectedVendor.BusinessName = model.BusinessName;
        selectedVendor.ContactPerson = model.ContactPerson;
        selectedVendor.Email = model.Email;
        if (model.Phone is null || (model.Phone.AreaCode is null && model.Phone.Number is null))
        {
            selectedVendor.Phone = null;
        }
        else if (selectedVendor.Phone is null)
        {
            selectedVendor.Phone = new PhoneNumber { AreaCode = model.Phone.AreaCode, Number = model.Phone.Number };
        }
        else
        {
            selectedVendor.Phone.AreaCode = model.Phone.AreaCode;
            selectedVendor.Phone.Number = model.Phone.Number;
        }
        selectedVendor.Website = model.Website;
        selectedVendor.ServiceType = model.ServiceType;
        selectedVendor.Address.Street = model.Address.Street;
        selectedVendor.Address.City = model.Address.City;
        selectedVendor.Address.State = model.Address.State;
        selectedVendor.Address.ZipCode = model.Address.ZipCode;
        selectedVendor.Description = model.Description;
        selectedVendor.AverageRating = model.AverageRating;
        selectedVendor.IsActive = model.IsActive;
        selectedVendor.IsFavorite = model.IsFavorite;
        selectedVendor.FacebookUrl = model.FacebookUrl;
        selectedVendor.InstagramUrl = model.InstagramUrl;

        await updateVendorHandler.HandleAsync(new UpdateVendorCommand(selectedVendor));
        return RedirectToAction("Index");
    }

    [HttpGet("Delete/{id:guid}")]
    public async Task<IActionResult> DeleteConfirmation(Guid id, CancellationToken cancellationToken)
    {
        var selectedVendor = await getVendorByIdHandler.HandleAsync(new GetVendorByIdQuery(id));
        return selectedVendor is null ? NotFound() : PartialView("_Delete", selectedVendor);
    }

    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var selectedVendor = await getVendorByIdHandler.HandleAsync(new GetVendorByIdQuery(id));
        if (selectedVendor is null)
        {
            return NotFound();
        }

        if ((await getAllVendorDocuments.HandleAsync()).Any(document => document.VendorId == id))
        {
            ModelState.AddModelError(string.Empty, "Elimina primero los documentos vinculados al proveedor.");
            return PartialView("_Delete", selectedVendor);
        }

        await deleteVendorHandler.HandleAsync(new DeleteVendorCommand(id));
        return RedirectToAction("Index");
    }

    private static VendorFormViewModel MapToViewModel(Vendor vendor)
    {
        return new VendorFormViewModel
        {
            BusinessName = vendor.BusinessName,
            ContactPerson = vendor.ContactPerson,
            Email = vendor.Email,
            Phone = vendor.Phone is null ? new PhoneNumber() : new PhoneNumber
            {
                AreaCode = vendor.Phone.AreaCode,
                Number = vendor.Phone.Number
            },
            Website = vendor.Website,
            ServiceType = vendor.ServiceType,
            Address = new Address
            {
                Street = vendor.Address.Street,
                City = vendor.Address.City,
                State = vendor.Address.State,
                ZipCode = vendor.Address.ZipCode
            },
            Description = vendor.Description,
            AverageRating = vendor.AverageRating,
            IsActive = vendor.IsActive,
            IsFavorite = vendor.IsFavorite,
            FacebookUrl = vendor.FacebookUrl,
            InstagramUrl = vendor.InstagramUrl
        };
    }
}