using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.GetWeddingByTitle;

public sealed class GetWeddingByTitleHandler(IWeddingService weddingService)
{
    public Task<IReadOnlyList<Wedding>?> HandleAsync(GetWeddingByTitleQuery query, CancellationToken cancellationToken = default) =>
        weddingService.GetByTitleAsync(query.Title, cancellationToken);
}