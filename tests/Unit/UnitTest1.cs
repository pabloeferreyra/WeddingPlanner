using FluentAssertions;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Tests.Unit;

public class WeddingTests
{
    [Fact]
    public void NewWeddingStartsInPlanningStatus()
    {
        var wedding = new Wedding();

        wedding.Status.Should().Be(WeddingStatus.Planning);
    }
}
