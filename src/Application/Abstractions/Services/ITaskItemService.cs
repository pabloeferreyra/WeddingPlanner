using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface ITaskItemService
{
    Task<IEnumerable<TaskItem>> GetAllTaskItemsAsync();
    Task<TaskItem> GetTaskItemByIdAsync(Guid taskItemId);
    Task<TaskItem> AddTaskItemAsync(TaskItem taskItem);
    Task<TaskItem> UpdateTaskItemAsync(TaskItem taskItem);
    Task DeleteTaskItemAsync(Guid taskItemId);
    Task<bool> TaskItemExistsAsync(Guid taskItemId);
}
