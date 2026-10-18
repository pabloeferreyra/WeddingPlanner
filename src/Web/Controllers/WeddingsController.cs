using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.CreateWedding;

namespace WeddingPlanner.Web.Controllers;

[ApiController]
[Route("api/weddings")]
public sealed class WeddingsController(CreateWeddingHandler createWeddingHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateWeddingCommand command)
    {
        var weddingId = await createWeddingHandler.HandleAsync(command);
        return StatusCode(StatusCodes.Status201Created, new { weddingId });
    }
}
