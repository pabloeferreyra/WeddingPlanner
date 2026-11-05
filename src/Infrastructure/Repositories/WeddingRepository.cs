using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class WeddingRepository(ApplicationDbContext dbContext, 
ILogger<WeddingRepository> logger) : IWeddingRepository
{
    public async Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting wedding with location {Id}", id);

        return await dbContext.Weddings
            .Include(wedding => wedding.Location)
            .FirstOrDefaultAsync(wedding => wedding.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Wedding>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting all weddings with locations");

        return await dbContext.Weddings
            .AsNoTracking()
            .Include(wedding => wedding.Location)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Wedding>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting the next {Limit} upcoming weddings", limit);

        var today = DateOnly.FromDateTime(DateTime.Today);

        var weddings = await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate >= today)
            .OrderBy(wedding => wedding.WeddingDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} upcoming weddings", weddings.Count);

        return weddings;
    }

    public async Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
       

        logger.LogDebug("Getting weddings by date {Date}", date);

        var weddings = await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate == date)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} weddings on {Date}", weddings?.Count ?? 0, date);

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