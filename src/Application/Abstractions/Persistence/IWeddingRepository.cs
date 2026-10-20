using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;

public interface IWeddingRepository
{
    Task<IReadOnlyList<Wedding>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Wedding>?> GetByTitle(string title, CancellationToken cancellationToken = default);
}