using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.GetUpcomingWeddings;
using WeddingPlanner.Application.UseCases.GetWeddingById;
using WeddingPlanner.Application.UseCases.CreateWedding;

namespace WeddingPlanner.Web.Controllers;

[ApiController]
[Route("api/weddings")]
public sealed class WeddingsController(CreateWeddingHandler createWeddingHandler,
GetUpcomingWeddingsHandler getUpcomingWeddingsHandler,
GetWeddingByIdHandler getWeddingByIdHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateWeddingCommand command)
    {
        var weddingId = await createWeddingHandler.HandleAsync(command);
        return StatusCode(StatusCodes.Status201Created, new { weddingId });
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(GetUpcomingWeddingsQuery query)
    {
        var weddings = await getUpcomingWeddingsHandler.Handle(query);
        return Ok(weddings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var wedding = await getWeddingByIdHandler.HandleAsync(new GetWeddingByIdQuery(id));
        return Ok(wedding);
    }

    

}
