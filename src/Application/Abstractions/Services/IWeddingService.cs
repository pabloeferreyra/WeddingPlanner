using WeddingPlanner.Application.Weddings;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IWeddingService
{
    Task<IReadOnlyList<WeddingSummary>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default);
}