using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Application.UseCases.ExistsWedding;

public sealed class ExistsWeddingHandler(IWeddingService weddingService)
{
    public Task<bool> HandleAsync(ExistsWeddingQuery query, CancellationToken cancellationToken = default) =>
        weddingService.ExistsAsync(query.WeddingId, cancellationToken);
}
