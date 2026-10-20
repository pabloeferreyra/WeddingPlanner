using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;

namespace WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

public sealed class GetUpcomingWeddingsHandler(IWeddingService service)
{
    public async Task<IReadOnlyList<WeddingSummary>> Handle(GetUpcomingWeddingsQuery query)
    {
        return await service.GetUpcomingAsync(query.Limit);
    }
}