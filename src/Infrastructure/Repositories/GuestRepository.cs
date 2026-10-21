using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public class GuestRepository(ApplicationDbContext context, 
ILogger<GuestRepository> logger) : IGuestRepository
{

    public async Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting guests for wedding {WeddingId}", weddingId);

        var guests = await context.Guests
            .Where(g => g.WeddingId == weddingId)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} guests for wedding {WeddingId}", guests.Count, weddingId);

        return guests;
    }

    public async Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting guests with status {Status} for wedding {WeddingId}", status, weddingId);

        var guests = await context.Guests
            .Where(g => g.WeddingId == weddingId && g.RSVPStatus == status)
            .ToListAsync(cancellationToken);

        logger.LogDebug("Retrieved {Count} guests with status {Status} for wedding {WeddingId}", guests.Count, status, weddingId);

        return guests;
    }
}