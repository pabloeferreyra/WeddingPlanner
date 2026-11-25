using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Web.Helpers;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

[Route("Weddings")]
public sealed class WeddingsController(
    CreateWeddingHandler createWeddingHandler,
    GetAllWeddingsHandler getAllWeddingsHandler,
    GetWeddingByIdHandler getWeddingByIdHandler,
    UpdateWeddingHandler updateWeddingHandler,
    DeleteWeddingHandler deleteWeddingHandler) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("InitializeTable")]
    public async Task<IActionResult> InitializeTable(CancellationToken cancellationToken)
    {
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);
        var weddings = await getAllWeddingsHandler.HandleAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var search = Request.Form["search"].FirstOrDefault()?.Trim();
        var status = Request.Form["status"].FirstOrDefault()?.Trim();
        var dateFilter = Request.Form["date"].FirstOrDefault()?.Trim();

        var filtered = weddings.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(wedding =>
                (wedding.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<WeddingStatus>(status, true, out var weddingStatus))
        {
            filtered = filtered.Where(wedding => wedding.Status == weddingStatus);
        }
        if (dateFilter == "upcoming")
        {
            filtered = filtered.Where(wedding => wedding.WeddingDate >= today);
        }
        else if (dateFilter == "past")
        {
            filtered = filtered.Where(wedding => wedding.WeddingDate < today);
        }

        var filteredList = filtered.ToList();
        return Json(new
        {
            data = DataTablesHelper.ApplyPaging(
                DataTablesHelper.ApplySorting(
                    filteredList.OrderBy(wedding => wedding.WeddingDate).ToList(),
                    Request),
                pageSize,
                skip),
            recordsTotal = weddings.Count(),
            recordsFiltered = filteredList.Count,
            summary = new
            {
                total = weddings.Count(),
                upcoming = weddings.Count(wedding => wedding.WeddingDate >= today),
                completed = weddings.Count(wedding => wedding.Status == WeddingStatus.Completed)
            }
        });
    }

    [HttpGet("Create")]
    public IActionResult Create() => PartialView("_Create", new WeddingFormViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WeddingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_Create", model);
        }

        var weddingId = await createWeddingHandler.HandleAsync(new CreateWeddingCommand
        {
            Title = model.Title,
            WeddingDate = model.WeddingDate,
            IsWedding = model.IsWedding,
            Bride = new PersonInfo(model.BrideName, model.BrideEmail, model.BridePhone),
            Groom = new PersonInfo(model.GroomName, model.GroomEmail, model.GroomPhone),
            Celebrated = new PersonInfo(model.CelebratedName, model.CelebratedEmail, model.CelebratedPhone),
            Street = model.Street,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            ExpectedGuestCount = model.ExpectedGuestCount,
            InitialBudget = model.InitialBudget
        });

        return RedirectToAction(nameof(Details), new { id = weddingId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await getWeddingByIdHandler.HandleAsync(
            new GetWeddingByIdQuery(id),
            cancellationToken);
        return wedding is null ? NotFound() : View(wedding);
    }

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await getWeddingByIdHandler.HandleAsync(
            new GetWeddingByIdQuery(id),
            cancellationToken);
        return wedding is null ? NotFound() : PartialView("_Edit", ToFormModel(wedding));
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        WeddingFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_Edit", model);
        }

        var wedding = await getWeddingByIdHandler.HandleAsync(
            new GetWeddingByIdQuery(id),
            cancellationToken);
        if (wedding is null)
        {
            return NotFound();
        }

        wedding.Title = model.Title;
        wedding.WeddingDate = model.WeddingDate;
        wedding.IsWedding = model.IsWedding;
        wedding.Status = model.Status;
        if (model.IsWedding)
        {
            wedding.Bride = new Person { Name = model.BrideName, Email = model.BrideEmail, Phone = model.BridePhone };
            wedding.Groom = new Person { Name = model.GroomName, Email = model.GroomEmail, Phone = model.GroomPhone };
            wedding.Celebrated = null;
        }
        else
        {
            wedding.Celebrated = new Person { Name = model.CelebratedName, Email = model.CelebratedEmail, Phone = model.CelebratedPhone };
            wedding.Bride = null;
            wedding.Groom = null;
        }
        if (wedding.Location is null)
        {
            wedding.Location = new Address
            {
                Street = model.Street,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode
            };
        }
        else
        {
            wedding.Location.Street = model.Street;
            wedding.Location.City = model.City;
            wedding.Location.State = model.State;
            wedding.Location.ZipCode = model.ZipCode;
        }
        wedding.ExpectedGuestCount = model.ExpectedGuestCount;
        wedding.InitialBudget = model.InitialBudget;

        await updateWeddingHandler.HandleAsync(new UpdateWeddingCommand(wedding), cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:guid}/Delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await getWeddingByIdHandler.HandleAsync(
            new GetWeddingByIdQuery(id),
            cancellationToken);
        return wedding is null ? NotFound() : View(wedding);
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await getWeddingByIdHandler.HandleAsync(
            new GetWeddingByIdQuery(id),
            cancellationToken);
        if (wedding is null)
        {
            return NotFound();
        }

        await deleteWeddingHandler.HandleAsync(new DeleteWeddingCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private static WeddingFormViewModel ToFormModel(Wedding wedding) => new()
    {
        Title = wedding.Title ?? string.Empty,
        WeddingDate = wedding.WeddingDate,
        Status = wedding.Status,
        IsWedding = wedding.IsWedding,
        BrideName = wedding.Bride?.Name ?? string.Empty,
        BrideEmail = wedding.Bride?.Email,
        BridePhone = wedding.Bride?.Phone,
        GroomName = wedding.Groom?.Name ?? string.Empty,
        GroomEmail = wedding.Groom?.Email,
        GroomPhone = wedding.Groom?.Phone,
        CelebratedName = wedding.Celebrated?.Name ?? string.Empty,
        CelebratedEmail = wedding.Celebrated?.Email,
        CelebratedPhone = wedding.Celebrated?.Phone,
        Street = wedding.Location?.Street ?? string.Empty,
        City = wedding.Location?.City ?? string.Empty,
        State = wedding.Location?.State ?? string.Empty,
        ZipCode = wedding.Location?.ZipCode ?? string.Empty,
        ExpectedGuestCount = wedding.ExpectedGuestCount,
        InitialBudget = wedding.InitialBudget
    };
}
