namespace WeddingPlanner.Core.Domain.Services;

public readonly record struct WeddingTableOccupancy(int Capacity, int Occupied)
{
    public int Available => Capacity - Occupied;
    public decimal Percentage => Capacity > 0 ? Occupied * 100m / Capacity : 0;

    public static WeddingTableOccupancy Calculate(int capacity, IEnumerable<Entities.Guest> guests)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        return new WeddingTableOccupancy(
            capacity,
            GuestAttendanceCalculator.Calculate(guests).ConfirmedAttendeeCount);
    }
}
