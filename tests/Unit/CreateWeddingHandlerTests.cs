using FluentAssertions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.UseCases.CreateWedding;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

public class CreateWeddingHandlerTests
{
    [Fact]
    public async Task HandleAsync_creates_and_persists_wedding_from_command()
    {
        var repository = new InMemoryWeddingRepository();
        var handler = new CreateWeddingHandler(repository);
        var command = new CreateWeddingCommand
        {
            Title = "Ana y Luis",
            WeddingDate = new DateTime(2027, 6, 12),
            BrideName = "Ana",
            GroomName = "Luis",
            Street = "Calle 1",
            City = "Buenos Aires",
            State = "CABA",
            ZipCode = "1000",
            ExpectedGuestCount = 80,
            InitialBudget = 25000
        };

        var weddingId = await handler.HandleAsync(command);

        weddingId.Should().NotBeEmpty();
        repository.SavedWedding.Should().NotBeNull();
        repository.SavedWedding!.Id.Should().Be(weddingId);
        repository.SavedWedding.Title.Should().Be(command.Title);
        repository.SavedWedding.WeddingDate.Should().Be(command.WeddingDate);
        repository.SavedWedding.BrideName.Should().Be(command.BrideName);
        repository.SavedWedding.GroomName.Should().Be(command.GroomName);
        repository.SavedWedding.Location.Should().BeEquivalentTo(new
        {
            command.Street,
            command.City,
            command.State,
            command.ZipCode
        });
        repository.SavedWedding.ExpectedGuestCount.Should().Be(command.ExpectedGuestCount);
        repository.SavedWedding.InitialBudget.Should().Be(command.InitialBudget);
    }

    private sealed class InMemoryWeddingRepository : IRepository<Wedding>
    {
        public Wedding? SavedWedding { get; private set; }

        public Task<Wedding> GetByIdAsync(Guid id) => Task.FromResult(SavedWedding!);
        public Task<IEnumerable<Wedding>> GetAllAsync() => Task.FromResult<IEnumerable<Wedding>>(
            SavedWedding is null ? [] : [SavedWedding]);

        public Task<Wedding> AddAsync(Wedding entity)
        {
            SavedWedding = entity;
            return Task.FromResult(entity);
        }

        public Task<Wedding> UpdateAsync(Wedding entity) => Task.FromResult(entity);
        public Task DeleteAsync(Guid id) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id) => Task.FromResult(SavedWedding?.Id == id);
        public IQueryable<Wedding> AsQueryable() => SavedWedding is null
            ? Enumerable.Empty<Wedding>().AsQueryable()
            : new[] { SavedWedding }.AsQueryable();
    }
}
