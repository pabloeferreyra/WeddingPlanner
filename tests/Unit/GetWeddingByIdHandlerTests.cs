using FluentAssertions;
using WeddingPlanner.Application.UseCases.Weddings;

namespace WeddingPlanner.Tests.Unit;

public class GetWeddingByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_wedding_when_it_exists()
    {
        var service = new InMemoryWeddingService();
        var wedding = TestWeddings.Create("Ana y Luis", new DateOnly(2027, 6, 12));
        service.Weddings.Add(wedding);
        var handler = new GetWeddingByIdHandler(service);

        var result = await handler.HandleAsync(new GetWeddingByIdQuery(wedding.Id));

        service.LastQueriedId.Should().Be(wedding.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(wedding.Id);
        result.Title.Should().Be("Ana y Luis");
        result.WeddingDate.Should().Be(new DateOnly(2027, 6, 12));
    }

    [Fact]
    public async Task HandleAsync_returns_null_when_wedding_does_not_exist()
    {
        var service = new InMemoryWeddingService();
        var handler = new GetWeddingByIdHandler(service);

        var result = await handler.HandleAsync(new GetWeddingByIdQuery(Guid.NewGuid()));

        result.Should().BeNull();
    }
}
