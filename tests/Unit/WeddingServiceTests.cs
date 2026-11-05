using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

public class WeddingServiceTests
{
    [Fact]
    public async Task GetUpcomingAsync_returns_summary_when_wedding_has_no_location()
    {
        var wedding = new Wedding
        {
            Title = "Ana y Luis",
            WeddingDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            Bride = new Person { Name = "Ana" },
            Groom = new Person { Name = "Luis" },
            Location = null
        };
        var service = new WeddingService(
            new StubWeddingRepository([wedding]),
            new StubRepository(),
            NullLogger<WeddingService>.Instance);

        var result = await service.GetUpcomingAsync(3);

        Assert.Equal("Location unavailable", Assert.Single(result).Venue);
    }

    private sealed class StubWeddingRepository(IReadOnlyList<Wedding> weddings) : IWeddingRepository
    {
        public Task<Wedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(weddings.FirstOrDefault(wedding => wedding.Id == id));

        public Task<IReadOnlyList<Wedding>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(weddings);

        public Task<IReadOnlyList<Wedding>> GetUpcomingAsync(int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult(weddings);

        public Task<IReadOnlyList<Wedding>?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Wedding>?> GetByTitle(string title, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class StubRepository : IRepository<Wedding>
    {
        public Task<Wedding> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IEnumerable<Wedding>> GetAllAsync() => throw new NotSupportedException();
        public Task<Wedding> AddAsync(Wedding entity) => throw new NotSupportedException();
        public Task<Wedding> UpdateAsync(Wedding entity) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(Guid id) => throw new NotSupportedException();
        public IQueryable<Wedding> AsQueryable() => throw new NotSupportedException();
    }
}
