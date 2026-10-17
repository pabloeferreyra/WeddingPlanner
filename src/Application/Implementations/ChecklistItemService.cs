using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class ChecklistItemService(IRepository<ChecklistItem> repository) : IChecklistItemService
{
    public async Task<IEnumerable<ChecklistItem>> GetAllChecklistItemsAsync() => await repository.GetAllAsync();
    public async Task<ChecklistItem> GetChecklistItemByIdAsync(Guid checklistItemId) => await repository.GetByIdAsync(checklistItemId);
    public async Task<ChecklistItem> AddChecklistItemAsync(ChecklistItem checklistItem) => await repository.AddAsync(checklistItem);
    public async Task<ChecklistItem> UpdateChecklistItemAsync(ChecklistItem checklistItem) => await repository.UpdateAsync(checklistItem);
    public async Task DeleteChecklistItemAsync(Guid checklistItemId) => await repository.DeleteAsync(checklistItemId);
    public async Task<bool> ChecklistItemExistsAsync(Guid checklistItemId) => await repository.ExistsAsync(checklistItemId);
}
