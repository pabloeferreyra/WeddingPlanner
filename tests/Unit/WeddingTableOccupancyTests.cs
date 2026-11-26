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

        Assert.Equal(3, occupancy.Occupied);
        Assert.Equal(3, occupancy.Available);
        Assert.Equal(50, occupancy.Percentage);
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
}
