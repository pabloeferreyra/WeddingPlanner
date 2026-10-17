using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class TaskItemService(IRepository<TaskItem> repository) : ITaskItemService
{
    public async Task<IEnumerable<TaskItem>> GetAllTaskItemsAsync() => await repository.GetAllAsync();
    public async Task<TaskItem> GetTaskItemByIdAsync(Guid taskItemId) => await repository.GetByIdAsync(taskItemId);
    public async Task<TaskItem> AddTaskItemAsync(TaskItem taskItem) => await repository.AddAsync(taskItem);
    public async Task<TaskItem> UpdateTaskItemAsync(TaskItem taskItem) => await repository.UpdateAsync(taskItem);
    public async Task DeleteTaskItemAsync(Guid taskItemId) => await repository.DeleteAsync(taskItemId);
    public async Task<bool> TaskItemExistsAsync(Guid taskItemId) => await repository.ExistsAsync(taskItemId);
}
