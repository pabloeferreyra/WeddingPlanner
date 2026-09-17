using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class WeddingRepository(ApplicationDbContext dbContext) : IWeddingRepository
{
    public async Task<IReadOnlyList<Wedding>> GetUpcomingAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Weddings
            .AsNoTracking()
            .OrderBy(wedding => wedding.Date)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}