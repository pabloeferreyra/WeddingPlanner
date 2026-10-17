using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Abstractions.Services;
public interface IGuestService
{
    Task<IEnumerable<Guest>> GetAllGuestsAsync();

    Task<Guest> GetGuestByIdAsync(Guid guestId);

    Task<Guest> AddGuestAsync(Guest guest);

    Task<Guest> UpdateGuestAsync(Guest guest);

    Task DeleteGuestAsync(Guid guestId);

    Task<bool> GuestExistsAsync(Guid guestId);
    Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default);
}