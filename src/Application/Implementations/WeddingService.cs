using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class WeddingService(IWeddingRepository weddingRepository, 
            IRepository<Wedding> genericRepository,
            ILogger<WeddingService> logger) : IWeddingService
{
    public async Task<IReadOnlyList<WeddingSummary>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        var weddings = await weddingRepository.GetUpcomingAsync(limit, cancellationToken);

        return [.. weddings.Select(wedding => new WeddingSummary(
            wedding.Id,
            wedding.Title!,
            wedding.WeddingDate,
            wedding.Location?.ToString() ?? "Location unavailable",
            wedding.ExpectedGuestCount,
            wedding.Status,
            wedding.IsWedding))];
    }

    public async Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await weddingRepository.GetByDateAsync(date, cancellationToken);
    }

    public async Task<IReadOnlyList<Wedding>?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await weddingRepository.GetByTitle(title, cancellationToken);
    }

    public async Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await weddingRepository.GetByIdAsync(id, cancellationToken);

    public async Task<IEnumerable<Wedding>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await weddingRepository.GetAllAsync(cancellationToken);

    public async Task<Wedding> AddAsync(Wedding wedding, CancellationToken cancellationToken = default) => await genericRepository.AddAsync(wedding);

    public async Task UpdateAsync(Wedding wedding, CancellationToken cancellationToken = default) => await genericRepository.UpdateAsync(wedding);

    public async Task DeleteAsync(Guid weddingId, CancellationToken cancellationToken = default) => await genericRepository.DeleteAsync(weddingId);

    public async Task<bool> ExistsAsync(Guid weddingId, CancellationToken cancellationToken = default) => await genericRepository.ExistsAsync(weddingId);
}