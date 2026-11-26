using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Core.Domain.Services;

namespace WeddingPlanner.Tests.Unit;

public class WeddingTableOccupancyTests
{
    [Fact]
    public void Occupancy_counts_only_confirmed_guests_and_their_valid_companions()
    {
        var guests = new[]
        {
            CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Confirmed, false, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Pending, true, CompanionAttendanceStatus.Confirmed),
            CreateGuest(GuestStatus.Declined, true, CompanionAttendanceStatus.Confirmed)
        };

        var occupancy = WeddingTableOccupancy.Calculate(6, guests);

        Assert.Equal(2, occupancy.Attendance.ConfirmedMainGuestCount);
        Assert.Equal(1, occupancy.Attendance.ConfirmedCompanionCount);
        Assert.Equal(3, occupancy.Occupied);
        Assert.Equal(3, occupancy.Available);
        Assert.Equal(50, occupancy.Percentage);
    }

    [Theory]
    [InlineData(GuestStatus.Confirmed, false, CompanionAttendanceStatus.Pending, 1, 0)]
    [InlineData(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed, 1, 1)]
    [InlineData(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Pending, 1, 0)]
    [InlineData(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Declined, 1, 0)]
    [InlineData(GuestStatus.Confirmed, false, CompanionAttendanceStatus.Confirmed, 1, 0)]
    [InlineData(GuestStatus.Pending, true, CompanionAttendanceStatus.Confirmed, 0, 0)]
    [InlineData(GuestStatus.Declined, true, CompanionAttendanceStatus.Confirmed, 0, 0)]
    public void Occupancy_breakdown_reflects_confirmed_attendance(
        GuestStatus status,
        bool canBringCompanion,
        CompanionAttendanceStatus companionStatus,
        int mainGuests,
        int companions)
    {
        var occupancy = WeddingTableOccupancy.Calculate(4, [CreateGuest(status, canBringCompanion, companionStatus)]);

        Assert.Equal(mainGuests, occupancy.Attendance.ConfirmedMainGuestCount);
        Assert.Equal(companions, occupancy.Attendance.ConfirmedCompanionCount);
        Assert.Equal(mainGuests + companions, occupancy.Occupied);
        Assert.Equal(4 - mainGuests - companions, occupancy.Available);
    }

    [Fact]
    public void Empty_and_full_tables_have_consistent_breakdowns()
    {
        var empty = WeddingTableOccupancy.Calculate(2, []);
        var full = WeddingTableOccupancy.Calculate(2,
            [CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed)]);

        Assert.Equal(0, empty.Attendance.ConfirmedMainGuestCount);
        Assert.Equal(0, empty.Attendance.ConfirmedCompanionCount);
        Assert.Equal(0, empty.Occupied);
        Assert.Equal(2, empty.Available);
        Assert.Equal(0, empty.Percentage);
        Assert.Equal(1, full.Attendance.ConfirmedMainGuestCount);
        Assert.Equal(1, full.Attendance.ConfirmedCompanionCount);
        Assert.Equal(2, full.Occupied);
        Assert.Equal(0, full.Available);
        Assert.Equal(100, full.Percentage);
    }

    [Fact]
    public async Task Seating_summary_includes_breakdown_for_each_table()
    {
        var weddingId = Guid.NewGuid();
        var table = new WeddingTable { WeddingId = weddingId, Name = "Principal", Capacity = 4 };
        var guest = CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Confirmed);
        guest.WeddingId = weddingId;
        guest.WeddingTableId = table.Id;
        var service = new WeddingTableService(new StubTableRepository(table, guest));

        var summary = await service.GetByWeddingAsync(weddingId);

        var occupancy = Assert.Single(summary.Tables).Occupancy;
        Assert.Equal(1, occupancy.Attendance.ConfirmedMainGuestCount);
        Assert.Equal(1, occupancy.Attendance.ConfirmedCompanionCount);
        Assert.Equal(2, occupancy.Occupied);
        Assert.Equal(2, occupancy.Available);
    }

    [Fact]
    public void Occupancy_changes_when_a_guest_or_companion_changes_status()
    {
        var guest = CreateGuest(GuestStatus.Confirmed, true, CompanionAttendanceStatus.Pending);
        Assert.Equal(1, WeddingTableOccupancy.Calculate(2, [guest]).Occupied);

        guest.CompanionStatus = CompanionAttendanceStatus.Confirmed;
        Assert.Equal(2, WeddingTableOccupancy.Calculate(2, [guest]).Occupied);

        guest.RSVPStatus = GuestStatus.Pending;
        Assert.Equal(0, WeddingTableOccupancy.Calculate(2, [guest]).Occupied);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Occupancy_requires_positive_capacity(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WeddingTableOccupancy.Calculate(capacity, []));
    }

    private static Guest CreateGuest(
        GuestStatus status,
        bool canBringCompanion,
        CompanionAttendanceStatus companionStatus) => new()
    {
        Wedding = new Wedding(),
        FirstName = "Ana",
        LastName = "Perez",
        Email = "ana@example.com",
        Notes = string.Empty,
        RSVPStatus = status,
        CanBringCompanion = canBringCompanion,
        CompanionStatus = companionStatus
    };

    private sealed class StubTableRepository(WeddingTable table, Guest guest) : IWeddingTableRepository
    {
        public Task<IReadOnlyList<WeddingTable>> GetByWeddingAsync(Guid weddingId) =>
            Task.FromResult<IReadOnlyList<WeddingTable>>([table]);
        public Task<IReadOnlyList<Guest>> GetGuestsByWeddingAsync(Guid weddingId) =>
            Task.FromResult<IReadOnlyList<Guest>>([guest]);
        public Task<T> WithWeddingLockAsync<T>(Guid weddingId, Func<Task<T>> action) => throw new NotSupportedException();
        public Task<WeddingTable?> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<Guest?> GetStoredGuestAsync(Guid id) => throw new NotSupportedException();
        public Task<WeddingTable> AddAsync(WeddingTable value) => throw new NotSupportedException();
        public Task<WeddingTable> UpdateAsync(WeddingTable value) => throw new NotSupportedException();
        public Task DeleteAsync(WeddingTable value) => throw new NotSupportedException();
    }
}
