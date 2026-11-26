using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;

public interface IWeddingTableRepository
{
    Task<T> WithWeddingLockAsync<T>(Guid weddingId, Func<Task<T>> action);
    Task<IReadOnlyList<WeddingTable>> GetByWeddingAsync(Guid weddingId);
    Task<WeddingTable?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Guest>> GetGuestsByWeddingAsync(Guid weddingId);
    Task<Guest?> GetStoredGuestAsync(Guid id);
    Task<WeddingTable> AddAsync(WeddingTable table);
    Task<WeddingTable> UpdateAsync(WeddingTable table);
    Task DeleteAsync(WeddingTable table);
}
