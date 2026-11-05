using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

/// <summary>
/// Factory for building valid Wedding entities in tests.
/// </summary>
public static class TestWeddings
{
    public static Wedding Create(
        string title,
        DateOnly weddingDate,
        string city = "Buenos Aires",
        int expectedGuestCount = 80,
        decimal initialBudget = 25000m) => new()
    {
        Title = title,
        WeddingDate = weddingDate,
        Bride = new Person { Name = "Ana" },
        Groom = new Person { Name = "Luis" },
        Location = new Address
        {
            Street = "Calle 1",
            City = city,
            State = "CABA",
            ZipCode = "1000"
        },
        ExpectedGuestCount = expectedGuestCount,
        InitialBudget = initialBudget
    };
}
