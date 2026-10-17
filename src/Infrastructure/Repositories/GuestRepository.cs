using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public class GuestRepository(ApplicationDbContext context) : IGuestRepository
{

    public async Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default) => await context.Guests
            .Where(g => g.WeddingId == weddingId)
            .ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default) => await context.Guests
            .Where(g => g.WeddingId == weddingId && g.RSVPStatus == status)
            .ToListAsync(cancellationToken);

}