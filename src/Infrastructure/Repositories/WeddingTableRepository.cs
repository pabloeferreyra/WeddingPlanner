using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public sealed class WeddingTableRepository(ApplicationDbContext context) : IWeddingTableRepository
{
    public async Task<T> WithWeddingLockAsync<T>(Guid weddingId, Func<Task<T>> action)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Weddings\" WHERE \"Id\" = {weddingId} FOR UPDATE");
        var result = await action();
        await transaction.CommitAsync();
        return result;
    }

    public async Task<IReadOnlyList<WeddingTable>> GetByWeddingAsync(Guid weddingId) =>
        await context.WeddingTables.AsNoTracking().Where(table => table.WeddingId == weddingId)
            .OrderBy(table => table.Name).ToListAsync();

    public Task<WeddingTable?> GetByIdAsync(Guid id) =>
        context.WeddingTables.AsNoTracking().FirstOrDefaultAsync(table => table.Id == id);

    public async Task<IReadOnlyList<Guest>> GetGuestsByWeddingAsync(Guid weddingId) =>
        await context.Guests.AsNoTracking().Where(guest => guest.WeddingId == weddingId).ToListAsync();

    public Task<Guest?> GetStoredGuestAsync(Guid id) =>
        context.Guests.AsNoTracking().FirstOrDefaultAsync(guest => guest.Id == id);

    public async Task<WeddingTable> AddAsync(WeddingTable table)
    {
        context.WeddingTables.Add(table);
        await context.SaveChangesAsync();
        return table;
    }

    public async Task<WeddingTable> UpdateAsync(WeddingTable table)
    {
        context.WeddingTables.Update(table);
        await context.SaveChangesAsync();
        return table;
    }

    public async Task DeleteAsync(WeddingTable table)
    {
        context.WeddingTables.Remove(table);
        await context.SaveChangesAsync();
    }
}
