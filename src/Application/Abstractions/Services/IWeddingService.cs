using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IWeddingService
{
    Task<IReadOnlyList<WeddingSummary>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default);
    
    Task<Wedding> AddAsync(Wedding wedding, CancellationToken cancellationToken = default);
    
    Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Wedding>?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Wedding>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(Wedding wedding, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid weddingId, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid weddingId, CancellationToken cancellationToken = default);
}