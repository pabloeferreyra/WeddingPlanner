using FluentAssertions;
using WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

namespace WeddingPlanner.Tests.Unit;

public class GetUpcomingWeddingsHandlerTests
{
    [Fact]
    public async Task Handle_forwards_limit_and_returns_only_upcoming_weddings_ordered_by_date()
    {
        var service = new InMemoryWeddingService();
        var today = DateTime.UtcNow.Date;
        var soon = TestWeddings.Create("Soon", today.AddDays(1));
        var later = TestWeddings.Create("Later", today.AddDays(2));
        service.Weddings.Add(later);
        service.Weddings.Add(TestWeddings.Create("Past", today.AddDays(-1)));
        service.Weddings.Add(soon);
        var handler = new GetUpcomingWeddingsHandler(service);

        var result = await handler.Handle(new GetUpcomingWeddingsQuery(2));

        service.LastUpcomingLimit.Should().Be(2);
        result.Should().HaveCount(2);
        result.Should().NotContain(summary => summary.Name == "Past");
        result[0].Id.Should().Be(soon.Id);
        result[1].Id.Should().Be(later.Id);
    }

    [Fact]
    public async Task Handle_maps_weddings_to_summaries_correctly()
    {
        var service = new InMemoryWeddingService();
        var date = DateTime.UtcNow.Date.AddDays(7);
        var wedding = TestWeddings.Create("Ana y Luis", date);
        service.Weddings.Add(wedding);
        var handler = new GetUpcomingWeddingsHandler(service);

        var result = await handler.Handle(new GetUpcomingWeddingsQuery(10));

        result.Should().ContainSingle();
        result[0].Id.Should().Be(wedding.Id);
        result[0].Name.Should().Be("Ana y Luis");
        result[0].Date.Should().Be(date);
        result[0].Venue.Should().Be("Calle 1, Buenos Aires, CABA 1000");
    }

    [Fact]
    public async Task Handle_excludes_past_weddings()
    {
        var service = new InMemoryWeddingService();
        var today = DateTime.UtcNow.Date;
        service.Weddings.Add(TestWeddings.Create("Past", today.AddDays(-1)));
        service.Weddings.Add(TestWeddings.Create("Future", today.AddDays(1)));
        var handler = new GetUpcomingWeddingsHandler(service);

        var result = await handler.Handle(new GetUpcomingWeddingsQuery(10));

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Future");
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(5, 3)]
    public async Task Handle_limits_results_to_requested_limit(int limit, int expectedCount)
    {
        var service = new InMemoryWeddingService();
        var today = DateTime.UtcNow.Date;
        service.Weddings.Add(TestWeddings.Create("Soon", today.AddDays(1)));
        service.Weddings.Add(TestWeddings.Create("Later", today.AddDays(2)));
        service.Weddings.Add(TestWeddings.Create("Much later", today.AddDays(3)));
        var handler = new GetUpcomingWeddingsHandler(service);

        var result = await handler.Handle(new GetUpcomingWeddingsQuery(limit));

        result.Should().HaveCount(expectedCount);
    }
}
