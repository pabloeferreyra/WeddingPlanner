using FluentAssertions;
using WeddingPlanner.Application.UseCases.Weddings;

namespace WeddingPlanner.Tests.Unit;

public class GetWeddingByDateHandlerTests
{
    [Fact]
    public async Task Handle_returns_only_weddings_on_requested_date()
    {
        var service = new InMemoryWeddingService();
        var date = new DateOnly(2027, 6, 12);
        var morningWedding = TestWeddings.Create("Ana y Luis", date);
        var eveningWedding = TestWeddings.Create("Val y Rafa", date);
        service.Weddings.Add(morningWedding);
        service.Weddings.Add(TestWeddings.Create("Otra boda", date.AddDays(1)));
        service.Weddings.Add(TestWeddings.Create("Otra más", date.AddDays(-1)));
        service.Weddings.Add(eveningWedding);
        var handler = new GetWeddingByDateHandler(service);

        var result = await handler.Handle(new GetWeddingByDateQuery(date));

        service.LastQueriedDate.Should().Be(date);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo([morningWedding, eveningWedding], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_wedding_matches_date()
    {
        var service = new InMemoryWeddingService();
        service.Weddings.Add(TestWeddings.Create("Ana y Luis", new DateOnly(2027, 6, 12)));
        var handler = new GetWeddingByDateHandler(service);

        var result = await handler.Handle(new GetWeddingByDateQuery(new DateOnly(2027, 12, 25)));

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
