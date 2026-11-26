using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Services;

public static class GuestAttendanceCalculator
{
    public static GuestAttendanceSummary Calculate(IEnumerable<Guest> guests)
    {
        var confirmedMainGuests = 0;
        var confirmedCompanions = 0;

        foreach (var guest in guests)
        {
            if (guest.RSVPStatus != GuestStatus.Confirmed)
            {
                continue;
            }

            confirmedMainGuests++;
            if (guest.CanBringCompanion && guest.CompanionStatus == CompanionAttendanceStatus.Confirmed)
            {
                confirmedCompanions++;
            }
        }

        return new GuestAttendanceSummary(confirmedMainGuests, confirmedCompanions);
    }
}

public readonly record struct GuestAttendanceSummary(int ConfirmedMainGuestCount, int ConfirmedCompanionCount)
{
    public int ConfirmedAttendeeCount => ConfirmedMainGuestCount + ConfirmedCompanionCount;
}
