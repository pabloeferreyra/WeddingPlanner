using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

public sealed class GetUpcomingWeddingsHandler(IWeddingRepository repository)
{
    public async Task<IEnumerable<Wedding>> Handle(GetUpcomingWeddingsQuery query)
    {
        return await repository.GetUpcomingAsync(query.Limit);
    }
}