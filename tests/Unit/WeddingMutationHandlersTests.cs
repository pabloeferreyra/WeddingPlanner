using FluentAssertions;
using WeddingPlanner.Application.UseCases.Weddings;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

public class WeddingMutationHandlersTests
{
    [Fact]
    public async Task UpdateWeddingHandler_updates_wedding()
    {
        var service = new InMemoryWeddingService();
        var wedding = new Wedding { Title = "Updated wedding" };
        service.Weddings.Add(wedding);
        var handler = new UpdateWeddingHandler(service);

        await handler.HandleAsync(new UpdateWeddingCommand(wedding));

        service.LastUpdatedWedding.Should().BeSameAs(wedding);
        service.Weddings.Should().ContainSingle().Which.Title.Should().Be("Updated wedding");
    }

    [Fact]
    public async Task DeleteWeddingHandler_deletes_wedding_by_id()
    {
        var service = new InMemoryWeddingService();
        var wedding = new Wedding();
        service.Weddings.Add(wedding);
        var handler = new DeleteWeddingHandler(service);

        await handler.HandleAsync(new DeleteWeddingCommand(wedding.Id));

        service.LastDeletedId.Should().Be(wedding.Id);
        service.Weddings.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsWeddingHandler_returns_whether_wedding_exists()
    {
        var service = new InMemoryWeddingService();
        var wedding = new Wedding();
        service.Weddings.Add(wedding);
        var handler = new ExistsWeddingHandler(service);

        var exists = await handler.HandleAsync(new ExistsWeddingQuery(wedding.Id));

        exists.Should().BeTrue();
        service.LastCheckedId.Should().Be(wedding.Id);
    }
}
