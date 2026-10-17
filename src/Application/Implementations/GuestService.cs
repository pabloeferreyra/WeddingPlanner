using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Implementations;

public sealed class GuestService(IRepository<Guest> genericRepository, 
IGuestRepository guestRepository, ILogger<GuestService> logger) : IGuestService
{
    public async Task<IEnumerable<Guest>> GetAllGuestsAsync() => await genericRepository.GetAllAsync();

    public async Task<Guest> GetGuestByIdAsync(Guid guestId) => await genericRepository.GetByIdAsync(guestId);

    public async Task<Guest> AddGuestAsync(Guest guest) => await genericRepository.AddAsync(guest);

    public async Task<Guest> UpdateGuestAsync(Guest guest) => await genericRepository.UpdateAsync(guest);

    public async Task DeleteGuestAsync(Guid guestId) => await genericRepository.DeleteAsync(guestId);

    public async Task<bool> GuestExistsAsync(Guid guestId) => await genericRepository.ExistsAsync(guestId);

    public async Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default) => await guestRepository.GetGuestsByWedding(weddingId, cancellationToken);

    public async Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default) => await guestRepository.GetGuestsByStatus(weddingId, status, cancellationToken);
}