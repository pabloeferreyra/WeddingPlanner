using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Services;

namespace WeddingPlanner.Application.Abstractions.Services;

public sealed record WeddingTableSummary(WeddingTable Table, IReadOnlyList<Guest> Guests, WeddingTableOccupancy Occupancy);

public sealed record WeddingSeatingSummary(
    IReadOnlyList<WeddingTableSummary> Tables,
    IReadOnlyList<Guest> UnassignedConfirmedGuests)
{
    public int TotalCapacity => Tables.Sum(table => table.Table.Capacity);
    public int Occupied => Tables.Sum(table => table.Occupancy.Occupied);
    public int Available => Tables.Sum(table => table.Occupancy.Available);
    public int FullTables => Tables.Count(table => table.Occupancy.Available == 0);
}

public interface IWeddingTableService
{
    Task<WeddingSeatingSummary> GetByWeddingAsync(Guid weddingId);
    Task<WeddingTable?> GetByIdAsync(Guid id);
    Task<WeddingTable> AddAsync(WeddingTable table);
    Task<WeddingTable> UpdateAsync(WeddingTable table);
    Task DeleteAsync(Guid weddingId, Guid tableId);
}
