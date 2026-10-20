using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.CreateWedding;

public sealed class CreateWeddingHandler(IWeddingService weddingService)
{
    public async Task<Guid> HandleAsync(CreateWeddingCommand command)
    {
        var wedding = new Wedding
        {
            Title = command.Title,
            WeddingDate = command.WeddingDate,
            BrideName = command.BrideName,
            GroomName = command.GroomName,
            Location = new Address
            {
                Street = command.Street,
                City = command.City,
                State = command.State,
                ZipCode = command.ZipCode
            },
            ExpectedGuestCount = command.ExpectedGuestCount,
            InitialBudget = command.InitialBudget
        };

        await weddingService.AddAsync(wedding);
        return wedding.Id;
    }
}