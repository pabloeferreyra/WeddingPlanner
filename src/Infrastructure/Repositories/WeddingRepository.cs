using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class WeddingRepository(ApplicationDbContext dbContext, 
ILogger<WeddingRepository> logger) : IWeddingRepository
{
    public async Task<IReadOnlyList<Wedding>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting the next {Limit} upcoming weddings", limit);

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);

        var weddings = await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate >= today)
            .OrderBy(wedding => wedding.WeddingDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} upcoming weddings", weddings.Count);

        return weddings;
    }

    public async Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var specifiedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);

        logger.LogDebug("Getting weddings by date {Date}", specifiedDate);

        var weddings = await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate == specifiedDate)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} weddings on {Date}", weddings?.Count ?? 0, specifiedDate);

        return weddings;
    }

    public async Task<IReadOnlyList<Wedding>?> GetByTitle(string title, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting weddings containing title {Title}", title);

        var weddings = await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.Title.Contains(title))
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} weddings matching title {Title}", weddings?.Count ?? 0, title);

        return weddings;
    }
}