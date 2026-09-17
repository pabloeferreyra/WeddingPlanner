using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;

public interface IWeddingRepository
{
    Task<IReadOnlyList<Wedding>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default);
}