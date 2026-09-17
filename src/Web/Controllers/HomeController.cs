using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

public class HomeController : Controller
{
    private readonly IWeddingService _weddingService;

    public HomeController(IWeddingService weddingService)
    {
        _weddingService = weddingService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var upcomingWeddings = await _weddingService.GetUpcomingAsync(3, cancellationToken);

        return View(upcomingWeddings);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
