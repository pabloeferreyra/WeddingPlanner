using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.TaskItems;

public sealed class GetAllTaskItemsHandler(ITaskItemService service)
{
    public Task<IEnumerable<TaskItem>> HandleAsync() => service.GetAllTaskItemsAsync();
}

public sealed record GetTaskItemByIdQuery(Guid TaskItemId);

public sealed class GetTaskItemByIdHandler(ITaskItemService service)
{
    public Task<TaskItem> HandleAsync(GetTaskItemByIdQuery query) => service.GetTaskItemByIdAsync(query.TaskItemId);
}

public sealed record AddTaskItemCommand(TaskItem TaskItem);

public sealed class AddTaskItemHandler(ITaskItemService service)
{
    public Task<TaskItem> HandleAsync(AddTaskItemCommand command) => service.AddTaskItemAsync(command.TaskItem);
}

public sealed record UpdateTaskItemCommand(TaskItem TaskItem);

public sealed class UpdateTaskItemHandler(ITaskItemService service)
{
    public Task<TaskItem> HandleAsync(UpdateTaskItemCommand command) => service.UpdateTaskItemAsync(command.TaskItem);
}

public sealed record DeleteTaskItemCommand(Guid TaskItemId);

public sealed class DeleteTaskItemHandler(ITaskItemService service)
{
    public Task HandleAsync(DeleteTaskItemCommand command) => service.DeleteTaskItemAsync(command.TaskItemId);
}

public sealed record TaskItemExistsQuery(Guid TaskItemId);

public sealed class TaskItemExistsHandler(ITaskItemService service)
{
    public Task<bool> HandleAsync(TaskItemExistsQuery query) => service.TaskItemExistsAsync(query.TaskItemId);
}
