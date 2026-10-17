using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Abstractions.Persistence;
public interface IGuestRepository
{
    Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default);
}