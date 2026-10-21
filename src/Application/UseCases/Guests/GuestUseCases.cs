using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.UseCases.Guests;

public sealed class GetAllGuestsHandler(IGuestService service)
{
    public Task<IEnumerable<Guest>> HandleAsync() => service.GetAllGuestsAsync();
}

public sealed record GetGuestByIdQuery(Guid GuestId);

public sealed class GetGuestByIdHandler(IGuestService service)
{
    public Task<Guest> HandleAsync(GetGuestByIdQuery query) => service.GetGuestByIdAsync(query.GuestId);
}

public sealed record AddGuestCommand(Guest Guest);

public sealed class AddGuestHandler(IGuestService service)
{
    public Task<Guest> HandleAsync(AddGuestCommand command) => service.AddGuestAsync(command.Guest);
}

public sealed record UpdateGuestCommand(Guest Guest);

public sealed class UpdateGuestHandler(IGuestService service)
{
    public Task<Guest> HandleAsync(UpdateGuestCommand command) => service.UpdateGuestAsync(command.Guest);
}

public sealed record DeleteGuestCommand(Guid GuestId);

public sealed class DeleteGuestHandler(IGuestService service)
{
    public Task HandleAsync(DeleteGuestCommand command) => service.DeleteGuestAsync(command.GuestId);
}

public sealed record GuestExistsQuery(Guid GuestId);

public sealed class GuestExistsHandler(IGuestService service)
{
    public Task<bool> HandleAsync(GuestExistsQuery query) => service.GuestExistsAsync(query.GuestId);
}

public sealed record GetGuestsByWeddingQuery(Guid WeddingId);

public sealed class GetGuestsByWeddingHandler(IGuestService service)
{
    public Task<IReadOnlyList<Guest>> HandleAsync(
        GetGuestsByWeddingQuery query,
        CancellationToken cancellationToken = default) =>
        service.GetGuestsByWedding(query.WeddingId, cancellationToken);
}

public sealed record GetGuestsByStatusQuery(Guid WeddingId, GuestStatus Status);

public sealed class GetGuestsByStatusHandler(IGuestService service)
{
    public Task<IReadOnlyList<Guest>> HandleAsync(
        GetGuestsByStatusQuery query,
        CancellationToken cancellationToken = default) =>
        service.GetGuestsByStatus(query.WeddingId, query.Status, cancellationToken);
}
