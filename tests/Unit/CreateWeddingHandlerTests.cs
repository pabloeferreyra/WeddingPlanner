using FluentAssertions;
using WeddingPlanner.Application.UseCases.Weddings;

namespace WeddingPlanner.Tests.Unit;

public class CreateWeddingHandlerTests
{
    [Fact]
    public async Task HandleAsync_creates_and_persists_wedding_from_command()
    {
        var service = new InMemoryWeddingService();
        var handler = new CreateWeddingHandler(service);
        var command = new CreateWeddingCommand
        {
            Title = "Ana y Luis",
            WeddingDate = new DateOnly(2027, 6, 12),
            Bride = new PersonInfo("Ana"),
            Groom = new PersonInfo("Luis"),
            Street = "Calle 1",
            City = "Buenos Aires",
            State = "CABA",
            ZipCode = "1000",
            ExpectedGuestCount = 80,
            InitialBudget = 25000
        };

        var weddingId = await handler.HandleAsync(command);

        weddingId.Should().NotBeEmpty();
        service.Weddings.Should().ContainSingle();
        var savedWedding = service.Weddings[0];
        savedWedding.Id.Should().Be(weddingId);
        savedWedding.Title.Should().Be(command.Title);
        savedWedding.WeddingDate.Should().Be(command.WeddingDate);
        savedWedding.Bride!.Name.Should().Be("Ana");
        savedWedding.Groom!.Name.Should().Be("Luis");
        savedWedding.Location.Should().BeEquivalentTo(new
        {
            command.Street,
            command.City,
            command.State,
            command.ZipCode
        });
        savedWedding.ExpectedGuestCount.Should().Be(command.ExpectedGuestCount);
        savedWedding.InitialBudget.Should().Be(command.InitialBudget);
    }
}
