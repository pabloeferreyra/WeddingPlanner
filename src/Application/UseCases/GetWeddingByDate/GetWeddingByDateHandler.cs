using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.GetWeddingByDate;

public class GetWeddingByDateHandler(
    IWeddingService weddingService)
{
    public async Task<IReadOnlyList<Wedding>?> Handle(GetWeddingByDateQuery query, CancellationToken cancellationToken = default) =>
        await weddingService.GetByDateAsync(query.Date, cancellationToken);
   
}