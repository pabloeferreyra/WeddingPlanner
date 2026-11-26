using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Core.Domain.Services;

namespace WeddingPlanner.Application.Implementations;

public sealed class WeddingTableService(IWeddingTableRepository repository) : IWeddingTableService
{
    public async Task<WeddingSeatingSummary> GetByWeddingAsync(Guid weddingId)
    {
        var tables = await repository.GetByWeddingAsync(weddingId);
        var guests = await repository.GetGuestsByWeddingAsync(weddingId);
        var assigned = guests.Where(guest => guest.WeddingTableId.HasValue)
            .ToLookup(guest => guest.WeddingTableId!.Value);
        return new WeddingSeatingSummary(
            tables.Select(table =>
            {
                var tableGuests = assigned[table.Id].ToList();
                return new WeddingTableSummary(
                    table, tableGuests, WeddingTableOccupancy.Calculate(table.Capacity, tableGuests));
            }).ToList(),
            guests.Where(guest => guest.WeddingTableId is null && guest.RSVPStatus == GuestStatus.Confirmed).ToList());
    }

    public Task<WeddingTable?> GetByIdAsync(Guid id) => repository.GetByIdAsync(id);

    public Task<WeddingTable> AddAsync(WeddingTable table) =>
        repository.WithWeddingLockAsync(table.WeddingId, async () =>
        {
            await ValidateAsync(table);
            return await repository.AddAsync(table);
        });

    public Task<WeddingTable> UpdateAsync(WeddingTable table) =>
        repository.WithWeddingLockAsync(table.WeddingId, async () =>
        {
            var existing = await repository.GetByIdAsync(table.Id);
            if (existing is null || existing.WeddingId != table.WeddingId)
                throw new ValidationException("No se encontró la mesa de esta celebración.");

            await ValidateAsync(table);
            var guests = await repository.GetGuestsByWeddingAsync(table.WeddingId);
            var occupied = GuestAttendanceCalculator.Calculate(
                guests.Where(guest => guest.WeddingTableId == table.Id)).ConfirmedAttendeeCount;
            if (table.Capacity < occupied)
                throw new ValidationException("La capacidad no puede ser menor a la ocupación actual.");

            return await repository.UpdateAsync(table);
        });

    public async Task DeleteAsync(Guid weddingId, Guid tableId)
    {
        await repository.WithWeddingLockAsync(weddingId, async () =>
        {
            var table = await repository.GetByIdAsync(tableId);
            if (table is null || table.WeddingId != weddingId)
                throw new ValidationException("No se encontró la mesa de esta celebración.");
            var guests = await repository.GetGuestsByWeddingAsync(weddingId);
            if (guests.Any(guest => guest.WeddingTableId == tableId))
                throw new ValidationException("Quita primero las asignaciones de esta mesa.");
            await repository.DeleteAsync(table);
            return true;
        });
    }

    private async Task ValidateAsync(WeddingTable table)
    {
        table.Name = table.Name?.Trim() ?? string.Empty;
        if (table.Name.Length is 0 or > 100)
            throw new ValidationException("El nombre de la mesa es obligatorio y no puede superar 100 caracteres.");
        if (table.Capacity <= 0)
            throw new ValidationException("La capacidad debe ser mayor que cero.");
        if (table.Notes?.Length > 500)
            throw new ValidationException("Las observaciones no pueden superar 500 caracteres.");
        var tables = await repository.GetByWeddingAsync(table.WeddingId);
        if (tables.Any(existing => existing.Id != table.Id &&
            string.Equals(existing.Name.Trim(), table.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException("Ya existe una mesa con ese nombre en esta celebración.");
    }
}
