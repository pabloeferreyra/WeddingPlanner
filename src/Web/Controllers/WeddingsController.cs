using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

[Route("Weddings")]
public sealed class WeddingsController(
    IWeddingService weddingService,
    CreateWeddingHandler createWeddingHandler) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var weddings = await weddingService.GetAllAsync(cancellationToken);
        return View(weddings.OrderBy(wedding => wedding.WeddingDate));
    }

    [HttpGet("Create")]
    public IActionResult Create() => PartialView("_Create", new WeddingFormViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WeddingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var weddingId = await createWeddingHandler.HandleAsync(new CreateWeddingCommand
        {
            Title = model.Title,
            WeddingDate = model.WeddingDate,
            BrideName = model.BrideName,
            GroomName = model.GroomName,
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
        var wedding = await weddingService.GetByIdAsync(id, cancellationToken);
        return wedding is null ? NotFound() : View(wedding);
    }

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await weddingService.GetByIdAsync(id, cancellationToken);
        return wedding is null ? NotFound() : View(ToFormModel(wedding));
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
            return View(model);
        }

        var wedding = await weddingService.GetByIdAsync(id, cancellationToken);
        if (wedding is null)
        {
            return NotFound();
        }

        wedding.Title = model.Title;
        wedding.WeddingDate = model.WeddingDate;
        wedding.BrideName = model.BrideName;
        wedding.GroomName = model.GroomName;
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

        await weddingService.UpdateAsync(wedding, cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:guid}/Delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await weddingService.GetByIdAsync(id, cancellationToken);
        return wedding is null ? NotFound() : View(wedding);
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var wedding = await weddingService.GetByIdAsync(id, cancellationToken);
        if (wedding is null)
        {
            return NotFound();
        }

        await weddingService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private static WeddingFormViewModel ToFormModel(Wedding wedding) => new()
    {
        Title = wedding.Title ?? string.Empty,
        WeddingDate = wedding.WeddingDate,
        BrideName = wedding.BrideName ?? string.Empty,
        GroomName = wedding.GroomName ?? string.Empty,
        Street = wedding.Location?.Street ?? string.Empty,
        City = wedding.Location?.City ?? string.Empty,
        State = wedding.Location?.State ?? string.Empty,
        ZipCode = wedding.Location?.ZipCode ?? string.Empty,
        ExpectedGuestCount = wedding.ExpectedGuestCount,
        InitialBudget = wedding.InitialBudget
    };
}
