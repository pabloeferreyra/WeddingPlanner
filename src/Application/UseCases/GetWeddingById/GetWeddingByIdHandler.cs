namespace WeddingPlanner.Application.UseCases.GetWeddingById;

using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
public sealed class GetWeddingByIdHandler(IWeddingService service)
{
    public async Task<Wedding?> HandleAsync(GetWeddingByIdQuery query)
    {
        return await service.GetByIdAsync(query.Id);
    }
}
