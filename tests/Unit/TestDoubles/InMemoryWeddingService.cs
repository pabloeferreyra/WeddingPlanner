using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

/// <summary>
/// In-memory implementation of IWeddingService for unit tests.
/// Records query parameters so tests can verify handlers delegate correctly.
/// </summary>
public sealed class InMemoryWeddingService : IWeddingService
{
    public List<Wedding> Weddings { get; } = [];

    public Guid? LastQueriedId { get; private set; }
    public DateTime? LastQueriedDate { get; private set; }
    public string? LastQueriedTitle { get; private set; }
    public int? LastUpcomingLimit { get; private set; }
    public Wedding? LastUpdatedWedding { get; private set; }
    public Guid? LastDeletedId { get; private set; }
    public Guid? LastCheckedId { get; private set; }

    public Task<Wedding> AddAsync(Wedding wedding, CancellationToken cancellationToken = default)
    {
        Weddings.Add(wedding);
        return Task.FromResult(wedding);
    }

    public Task<IEnumerable<Wedding>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<Wedding>>(Weddings);

    public Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LastQueriedId = id;
        return Task.FromResult(Weddings.FirstOrDefault(wedding => wedding.Id == id));
    }

    public Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        LastQueriedDate = date;

        IReadOnlyList<Wedding> result = Weddings
            .Where(wedding => wedding.WeddingDate == date.Date)
            .OrderBy(wedding => wedding.WeddingDate)
            .ToList();

        return Task.FromResult<IReadOnlyList<Wedding>?>(result);
    }

    public Task<IReadOnlyList<Wedding>?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        LastQueriedTitle = title;

        IReadOnlyList<Wedding> result = Weddings
            .Where(wedding => wedding.Title != null && wedding.Title.Contains(title))
            .ToList();

        return Task.FromResult<IReadOnlyList<Wedding>?>(result);
    }

    public Task<IReadOnlyList<WeddingSummary>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default)
    {
        LastUpcomingLimit = limit;

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);

        IReadOnlyList<WeddingSummary> result = Weddings
            .Where(wedding => wedding.WeddingDate >= today)
            .OrderBy(wedding => wedding.WeddingDate)
            .Take(limit)
            .Select(wedding => new WeddingSummary(
                wedding.Id,
                wedding.Title!,
                wedding.WeddingDate,
                wedding.Location!.ToString()))
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(Wedding wedding, CancellationToken cancellationToken = default)
    {
        LastUpdatedWedding = wedding;
        var existingIndex = Weddings.FindIndex(existing => existing.Id == wedding.Id);
        if (existingIndex >= 0)
        {
            Weddings[existingIndex] = wedding;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid weddingId, CancellationToken cancellationToken = default)
    {
        LastDeletedId = weddingId;
        Weddings.RemoveAll(wedding => wedding.Id == weddingId);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid weddingId, CancellationToken cancellationToken = default)
    {
        LastCheckedId = weddingId;
        return Task.FromResult(Weddings.Any(wedding => wedding.Id == weddingId));
    }
}
