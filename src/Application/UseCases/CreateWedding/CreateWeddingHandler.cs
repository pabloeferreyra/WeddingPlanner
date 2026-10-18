using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.CreateWedding;

public sealed class CreateWeddingHandler(IRepository<Wedding> weddingRepository)
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

        await weddingRepository.AddAsync(wedding);
        return wedding.Id;
    }
}