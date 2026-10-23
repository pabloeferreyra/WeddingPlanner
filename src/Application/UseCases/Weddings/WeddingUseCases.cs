using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Weddings;

public sealed class CreateWeddingCommand
{
    [Required]
    [StringLength(150)]
    public required string Title { get; init; }

    [Required]
    public required DateTime WeddingDate { get; init; }

    [Required]
    [StringLength(100)]
    public required string BrideName { get; init; }

    [Required]
    [StringLength(100)]
    public required string GroomName { get; init; }

    [Required]
    [StringLength(200)]
    public required string Street { get; init; }

    [Required]
    [StringLength(100)]
    public required string City { get; init; }

    [Required]
    [StringLength(100)]
    public required string State { get; init; }

    [Required]
    [StringLength(20)]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Codigo postal invalido.")]
    [MinLength(4)]
    [DataType(DataType.PostalCode)]
    public required string ZipCode { get; init; }

    [Range(0, int.MaxValue)]
    public int ExpectedGuestCount { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal InitialBudget { get; init; }
}

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

public sealed class GetAllWeddingsHandler(IWeddingService weddingService)
{
    public Task<IEnumerable<Wedding>> HandleAsync(CancellationToken cancellationToken = default) =>
        weddingService.GetAllAsync(cancellationToken);
}

public sealed class GetUpcomingWeddingsQuery(int limit)
{
    [Required]
    public int Limit { get; } = limit;
}

public sealed class GetUpcomingWeddingsHandler(IWeddingService service)
{
    public Task<IReadOnlyList<WeddingSummary>> Handle(GetUpcomingWeddingsQuery query) =>
        service.GetUpcomingAsync(query.Limit);
}

public sealed record GetWeddingByDateQuery(DateTime Date);

public sealed class GetWeddingByDateHandler(IWeddingService weddingService)
{
    public Task<IReadOnlyList<Wedding>?> Handle(
        GetWeddingByDateQuery query,
        CancellationToken cancellationToken = default) =>
        weddingService.GetByDateAsync(query.Date, cancellationToken);
}

public sealed record GetWeddingByIdQuery(Guid Id);

public sealed class GetWeddingByIdHandler(IWeddingService service)
{
    public Task<Wedding?> HandleAsync(GetWeddingByIdQuery query) =>
        service.GetByIdAsync(query.Id);
}

public sealed record GetWeddingByTitleQuery(string Title);

public sealed class GetWeddingByTitleHandler(IWeddingService weddingService)
{
    public Task<IReadOnlyList<Wedding>?> HandleAsync(
        GetWeddingByTitleQuery query,
        CancellationToken cancellationToken = default) =>
        weddingService.GetByTitleAsync(query.Title, cancellationToken);
}

public sealed record UpdateWeddingCommand(Wedding Wedding);

public sealed class UpdateWeddingHandler(IWeddingService weddingService)
{
    public Task HandleAsync(UpdateWeddingCommand command, CancellationToken cancellationToken = default) =>
        weddingService.UpdateAsync(command.Wedding, cancellationToken);
}

public sealed record DeleteWeddingCommand(Guid WeddingId);

public sealed class DeleteWeddingHandler(IWeddingService weddingService)
{
    public Task HandleAsync(DeleteWeddingCommand command, CancellationToken cancellationToken = default) =>
        weddingService.DeleteAsync(command.WeddingId, cancellationToken);
}

public sealed record ExistsWeddingQuery(Guid WeddingId);

public sealed class ExistsWeddingHandler(IWeddingService weddingService)
{
    public Task<bool> HandleAsync(ExistsWeddingQuery query, CancellationToken cancellationToken = default) =>
        weddingService.ExistsAsync(query.WeddingId, cancellationToken);
}
