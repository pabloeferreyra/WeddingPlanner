using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class WeddingRepository(ApplicationDbContext dbContext) : 
IWeddingRepository
{
    public async Task<IReadOnlyList<Wedding>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);

        return await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate >= today)
            .OrderBy(wedding => wedding.WeddingDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var specifiedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);

        return await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.WeddingDate == specifiedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Wedding>?> GetByTitle(string title, CancellationToken cancellationToken = default)
    {
        return await dbContext.Weddings
            .AsNoTracking()
            .Where(wedding => wedding.Title.Contains(title))
            .ToListAsync(cancellationToken);
    }
}