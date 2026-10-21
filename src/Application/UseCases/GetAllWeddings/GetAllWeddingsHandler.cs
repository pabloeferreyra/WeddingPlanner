using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.GetAllWeddings;

public class GetAllWeddingsHandler(IWeddingService weddingService)
{
    public Task<IEnumerable<Wedding>> HandleAsync(CancellationToken cancellationToken = default) =>
        weddingService.GetAllAsync(cancellationToken);
}