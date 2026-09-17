using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;

namespace WeddingPlanner.Application.Weddings;

public sealed class WeddingService(IWeddingRepository weddingRepository) : IWeddingService
{
    public async Task<IReadOnlyList<WeddingSummary>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var weddings = await weddingRepository.GetUpcomingAsync(limit, cancellationToken);

        return weddings
            .Select(wedding => new WeddingSummary(wedding.Name, wedding.Date, wedding.Venue))
            .ToList();
    }
}