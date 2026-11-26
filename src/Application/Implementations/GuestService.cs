using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Core.Domain.Services;

namespace WeddingPlanner.Application.Implementations;

public sealed class GuestService(IRepository<Guest> genericRepository, 
IGuestRepository guestRepository, ILogger<GuestService> logger,
IWeddingTableRepository tableRepository) : IGuestService
{
    public async Task<IEnumerable<Guest>> GetAllGuestsAsync() => await genericRepository.GetAllAsync();

    public async Task<Guest> GetGuestByIdAsync(Guid guestId) => await genericRepository.GetByIdAsync(guestId);

    public async Task<Guest> AddGuestAsync(Guest guest)
    {
        NormalizeCompanionAttendance(guest);
        return await tableRepository.WithWeddingLockAsync(guest.WeddingId, async () =>
        {
            if (guest.WeddingTableId is not null)
                await ValidateTableAsync(guest, isNew: true);
            return await genericRepository.AddAsync(guest);
        });
    }

    public async Task<Guest> UpdateGuestAsync(Guest guest)
    {
        NormalizeCompanionAttendance(guest);
        return await tableRepository.WithWeddingLockAsync(guest.WeddingId, async () =>
        {
            if (guest.WeddingTableId is not null)
                await ValidateTableAsync(guest, isNew: false);
            return await genericRepository.UpdateAsync(guest);
        });
    }

    public async Task DeleteGuestAsync(Guid guestId)
    {
        var guest = await genericRepository.GetByIdAsync(guestId);
        if (guest is null) return;
        await tableRepository.WithWeddingLockAsync(guest.WeddingId, async () =>
        {
            await genericRepository.DeleteAsync(guestId);
            return true;
        });
    }

    public async Task<bool> GuestExistsAsync(Guid guestId) => await genericRepository.ExistsAsync(guestId);

    public async Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default) => await guestRepository.GetGuestsByWedding(weddingId, cancellationToken);

    public async Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default) => await guestRepository.GetGuestsByStatus(weddingId, status, cancellationToken);

    private async Task ValidateTableAsync(Guest guest, bool isNew)
    {
        var table = await tableRepository.GetByIdAsync(guest.WeddingTableId!.Value);
        if (table is null || table.WeddingId != guest.WeddingId)
            throw new ValidationException("Selecciona una mesa de esta celebración.");

        var storedGuest = isNew ? null : await tableRepository.GetStoredGuestAsync(guest.Id);
        if (!isNew && (storedGuest is null || storedGuest.WeddingId != guest.WeddingId))
            throw new ValidationException("No se encontró el invitado de esta celebración.");
        if (guest.RSVPStatus != GuestStatus.Confirmed &&
            (isNew || storedGuest!.WeddingTableId != guest.WeddingTableId))
            throw new ValidationException("Solo se pueden asignar invitados confirmados.");

        var guests = await tableRepository.GetGuestsByWeddingAsync(guest.WeddingId);
        var others = guests.Where(existing =>
            existing.WeddingTableId == table.Id && existing.Id != guest.Id);
        var occupied = GuestAttendanceCalculator.Calculate(others.Append(guest)).ConfirmedAttendeeCount;
        if (occupied > table.Capacity)
            throw new ValidationException("La mesa no tiene suficientes lugares disponibles.");
    }

    private static void NormalizeCompanionAttendance(Guest guest)
    {
        if (!guest.CanBringCompanion)
        {
            guest.CompanionStatus = CompanionAttendanceStatus.Pending;
            guest.CompanionName = null;
            guest.CompanionMealPreference = null;
            guest.CompanionDietaryRestrictions = null;
            guest.CompanionNotes = null;
        }
        else if (guest.RSVPStatus == GuestStatus.Declined)
        {
            guest.CompanionStatus = CompanionAttendanceStatus.Declined;
        }
    }
}