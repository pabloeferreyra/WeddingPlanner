using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.ChecklistItems;

public sealed class GetAllChecklistItemsHandler(IChecklistItemService service)
{
    public Task<IEnumerable<ChecklistItem>> HandleAsync() => service.GetAllChecklistItemsAsync();
}

public sealed record GetChecklistItemByIdQuery(Guid ChecklistItemId);

public sealed class GetChecklistItemByIdHandler(IChecklistItemService service)
{
    public Task<ChecklistItem> HandleAsync(GetChecklistItemByIdQuery query) =>
        service.GetChecklistItemByIdAsync(query.ChecklistItemId);
}

public sealed record AddChecklistItemCommand(ChecklistItem ChecklistItem);

public sealed class AddChecklistItemHandler(IChecklistItemService service)
{
    public Task<ChecklistItem> HandleAsync(AddChecklistItemCommand command) =>
        service.AddChecklistItemAsync(command.ChecklistItem);
}

public sealed record UpdateChecklistItemCommand(ChecklistItem ChecklistItem);

public sealed class UpdateChecklistItemHandler(IChecklistItemService service)
{
    public Task<ChecklistItem> HandleAsync(UpdateChecklistItemCommand command) =>
        service.UpdateChecklistItemAsync(command.ChecklistItem);
}

public sealed record DeleteChecklistItemCommand(Guid ChecklistItemId);

public sealed class DeleteChecklistItemHandler(IChecklistItemService service)
{
    public Task HandleAsync(DeleteChecklistItemCommand command) =>
        service.DeleteChecklistItemAsync(command.ChecklistItemId);
}

public sealed record ChecklistItemExistsQuery(Guid ChecklistItemId);

public sealed class ChecklistItemExistsHandler(IChecklistItemService service)
{
    public Task<bool> HandleAsync(ChecklistItemExistsQuery query) =>
        service.ChecklistItemExistsAsync(query.ChecklistItemId);
}
