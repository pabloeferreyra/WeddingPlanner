using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Tests.Unit;

public class WeddingTableServiceTests
{
    [Fact]
    public async Task Creating_table_validates_capacity_and_name_within_wedding()
    {
        var store = new SeatingStore();
        var service = new WeddingTableService(store);
        var weddingId = Guid.NewGuid();
        var table = new WeddingTable { WeddingId = weddingId, Name = " Mesa 1 ", Capacity = 2 };

        await service.AddAsync(table);
        Assert.Equal("Mesa 1", table.Name);
        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(
            new WeddingTable { WeddingId = weddingId, Name = "mesa 1", Capacity = 2 }));
        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(
            new WeddingTable { WeddingId = weddingId, Name = "Mesa 2", Capacity = 0 }));
        await service.AddAsync(new WeddingTable { WeddingId = Guid.NewGuid(), Name = "Mesa 1", Capacity = 2 });
    }

    [Fact]
    public async Task Assigned_table_cannot_be_deleted_or_reduced_below_occupancy()
    {
        var store = new SeatingStore();
        var service = new WeddingTableService(store);
        var table = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "1", Capacity = 2 };
        await service.AddAsync(table);
        store.Guests.Add(CreateGuest(table.WeddingId, table.Id, true));

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(
            new WeddingTable { Id = table.Id, WeddingId = table.WeddingId, Name = table.Name, Capacity = 1 }));
        await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(table.WeddingId, table.Id));

        store.Guests.Clear();
        await service.DeleteAsync(table.WeddingId, table.Id);
        Assert.Empty(store.Tables);
    }

    [Fact]
    public async Task Assigning_and_moving_guest_respects_capacity_and_wedding()
    {
        var store = new SeatingStore();
        var weddingId = Guid.NewGuid();
        var first = new WeddingTable { WeddingId = weddingId, Name = "1", Capacity = 2 };
        var second = new WeddingTable { WeddingId = weddingId, Name = "2", Capacity = 1 };
        var other = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "3", Capacity = 4 };
        store.Tables.AddRange([first, second, other]);
        var guests = new GuestService(store, store, NullLogger<GuestService>.Instance, store);
        var guest = CreateGuest(weddingId, first.Id, true);

        await guests.AddGuestAsync(guest);
        Assert.Equal(2, (await new WeddingTableService(store).GetByWeddingAsync(weddingId)).Tables
            .Single(table => table.Table.Id == first.Id).Occupancy.Occupied);
        guest.WeddingTableId = second.Id;
        await Assert.ThrowsAsync<ValidationException>(() => guests.UpdateGuestAsync(guest));
        guest.WeddingTableId = other.Id;
        await Assert.ThrowsAsync<ValidationException>(() => guests.UpdateGuestAsync(guest));
        guest.WeddingTableId = second.Id;
        guest.CompanionStatus = CompanionAttendanceStatus.Pending;
        await guests.UpdateGuestAsync(guest);

        var summary = await new WeddingTableService(store).GetByWeddingAsync(weddingId);
        Assert.Equal(0, summary.Tables.Single(table => table.Table.Id == first.Id).Occupancy.Occupied);
        Assert.Equal(1, summary.Tables.Single(table => table.Table.Id == second.Id).Occupancy.Occupied);
    }

    [Fact]
    public async Task Rsvp_and_companion_changes_recheck_capacity_and_unassigned_list()
    {
        var store = new SeatingStore();
        var weddingId = Guid.NewGuid();
        var table = new WeddingTable { WeddingId = weddingId, Name = "1", Capacity = 1 };
        store.Tables.Add(table);
        var guests = new GuestService(store, store, NullLogger<GuestService>.Instance, store);
        var guest = CreateGuest(weddingId, table.Id, false);
        await guests.AddGuestAsync(guest);
        guest.CanBringCompanion = true;
        guest.CompanionStatus = CompanionAttendanceStatus.Confirmed;
        await Assert.ThrowsAsync<ValidationException>(() => guests.UpdateGuestAsync(guest));

        guest.RSVPStatus = GuestStatus.Pending;
        await guests.UpdateGuestAsync(guest);
        var summary = await new WeddingTableService(store).GetByWeddingAsync(weddingId);
        Assert.Equal(0, summary.Occupied);
        Assert.Null(store.Guests.Single().WeddingTableId);
        guest.RSVPStatus = GuestStatus.Confirmed;
        await guests.UpdateGuestAsync(guest);
        summary = await new WeddingTableService(store).GetByWeddingAsync(weddingId);
        Assert.Single(summary.UnassignedConfirmedGuests);
        guest.WeddingTableId = table.Id;
        await Assert.ThrowsAsync<ValidationException>(() => guests.UpdateGuestAsync(guest));
        Assert.Null(store.Guests.Single().WeddingTableId);
    }

    [Theory]
    [InlineData(GuestStatus.Pending)]
    [InlineData(GuestStatus.Declined)]
    public async Task Unconfirmed_guest_loses_table_and_does_not_recover_it_when_reconfirmed(GuestStatus status)
    {
        var store = new SeatingStore();
        var table = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "1", Capacity = 2 };
        store.Tables.Add(table);
        var service = new GuestService(store, store, NullLogger<GuestService>.Instance, store);
        var guest = CreateGuest(table.WeddingId, table.Id, false);
        await service.AddGuestAsync(guest);

        guest.RSVPStatus = status;
        await service.UpdateGuestAsync(guest);

        Assert.Null(guest.WeddingTableId);
        Assert.Null(store.Guests.Single().WeddingTableId);
        Assert.Equal(status, store.Guests.Single().RSVPStatus);

        guest.RSVPStatus = GuestStatus.Confirmed;
        await service.UpdateGuestAsync(guest);

        Assert.Null(store.Guests.Single().WeddingTableId);
    }

    [Fact]
    public async Task Guest_remaining_confirmed_keeps_table()
    {
        var store = new SeatingStore();
        var table = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "1", Capacity = 1 };
        store.Tables.Add(table);
        var service = new GuestService(store, store, NullLogger<GuestService>.Instance, store);
        var guest = CreateGuest(table.WeddingId, table.Id, false);
        await service.AddGuestAsync(guest);

        await service.UpdateGuestAsync(guest);

        Assert.Equal(table.Id, store.Guests.Single().WeddingTableId);
    }

    [Theory]
    [InlineData(GuestStatus.Pending)]
    [InlineData(GuestStatus.Declined)]
    public async Task Cannot_assign_unconfirmed_guests(GuestStatus status)
    {
        var store = new SeatingStore();
        var table = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "1", Capacity = 2 };
        store.Tables.Add(table);
        var guest = CreateGuest(table.WeddingId, table.Id, false);
        guest.RSVPStatus = status;
        var service = new GuestService(store, store, NullLogger<GuestService>.Instance, store);

        await Assert.ThrowsAsync<ValidationException>(() => service.AddGuestAsync(guest));
    }

    [Fact]
    public async Task Concurrent_assignments_cannot_both_claim_the_last_place()
    {
        var store = new SeatingStore();
        var table = new WeddingTable { WeddingId = Guid.NewGuid(), Name = "1", Capacity = 1 };
        store.Tables.Add(table);
        var service = new GuestService(store, store, NullLogger<GuestService>.Instance, store);
        var first = CreateGuest(table.WeddingId, table.Id, false);
        var second = CreateGuest(table.WeddingId, table.Id, false);

        async Task<bool> TryAssign(Guest guest)
        {
            try
            {
                await service.AddGuestAsync(guest);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }
        }

        var results = await Task.WhenAll(TryAssign(first), TryAssign(second));

        Assert.Single(results, result => result);
        Assert.Single(store.Guests);
    }

    private static Guest CreateGuest(Guid weddingId, Guid? tableId, bool companion) => new()
    {
        WeddingId = weddingId,
        Wedding = new Wedding(),
        FirstName = "Ana",
        LastName = "Perez",
        Email = "ana@example.com",
        Notes = string.Empty,
        RSVPStatus = GuestStatus.Confirmed,
        WeddingTableId = tableId,
        CanBringCompanion = companion,
        CompanionStatus = companion ? CompanionAttendanceStatus.Confirmed : CompanionAttendanceStatus.Pending
    };

    private sealed class SeatingStore : IWeddingTableRepository, IRepository<Guest>, IGuestRepository
    {
        private readonly SemaphoreSlim mutex = new(1, 1);
        public List<WeddingTable> Tables { get; } = [];
        public List<Guest> Guests { get; } = [];

        public async Task<T> WithWeddingLockAsync<T>(Guid weddingId, Func<Task<T>> action)
        {
            await mutex.WaitAsync();
            try
            {
                await Task.Yield();
                return await action();
            }
            finally
            {
                mutex.Release();
            }
        }
        public Task<IReadOnlyList<WeddingTable>> GetByWeddingAsync(Guid weddingId) =>
            Task.FromResult<IReadOnlyList<WeddingTable>>(Tables.Where(table => table.WeddingId == weddingId).ToList());
        public Task<WeddingTable?> GetByIdAsync(Guid id) =>
            Task.FromResult(Tables.FirstOrDefault(table => table.Id == id));
        public Task<IReadOnlyList<Guest>> GetGuestsByWeddingAsync(Guid weddingId) =>
            Task.FromResult<IReadOnlyList<Guest>>(Guests.Where(guest => guest.WeddingId == weddingId).Select(Clone).ToList());
        public Task<Guest?> GetStoredGuestAsync(Guid id) =>
            Task.FromResult(Guests.FirstOrDefault(guest => guest.Id == id) is { } guest ? Clone(guest) : null);
        public Task<WeddingTable> AddAsync(WeddingTable table) { Tables.Add(table); return Task.FromResult(table); }
        public Task<WeddingTable> UpdateAsync(WeddingTable table) { Tables.RemoveAll(item => item.Id == table.Id); Tables.Add(table); return Task.FromResult(table); }
        public Task DeleteAsync(WeddingTable table) { Tables.Remove(table); return Task.CompletedTask; }
        public Task<Guest> AddAsync(Guest guest) { Guests.Add(Clone(guest)); return Task.FromResult(guest); }
        public Task<Guest> UpdateAsync(Guest guest) { Guests.RemoveAll(item => item.Id == guest.Id); Guests.Add(Clone(guest)); return Task.FromResult(guest); }
        Task<Guest> IRepository<Guest>.GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IEnumerable<Guest>> GetAllAsync() => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(Guid id) => throw new NotSupportedException();
        public IQueryable<Guest> AsQueryable() => throw new NotSupportedException();
        public Task<IReadOnlyList<Guest>> GetGuestsByWedding(Guid weddingId, CancellationToken cancellationToken = default) => GetGuestsByWeddingAsync(weddingId);
        public Task<IReadOnlyList<Guest>> GetGuestsByStatus(Guid weddingId, GuestStatus status, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private static Guest Clone(Guest guest) => new()
        {
            Id = guest.Id,
            WeddingId = guest.WeddingId,
            Wedding = guest.Wedding,
            FirstName = guest.FirstName,
            LastName = guest.LastName,
            Email = guest.Email,
            Notes = guest.Notes,
            RSVPStatus = guest.RSVPStatus,
            WeddingTableId = guest.WeddingTableId,
            CanBringCompanion = guest.CanBringCompanion,
            CompanionStatus = guest.CompanionStatus
        };
    }
}
