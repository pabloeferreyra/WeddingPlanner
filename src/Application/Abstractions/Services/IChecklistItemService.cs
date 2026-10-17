using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IChecklistItemService
{
    Task<IEnumerable<ChecklistItem>> GetAllChecklistItemsAsync();
    Task<ChecklistItem> GetChecklistItemByIdAsync(Guid checklistItemId);
    Task<ChecklistItem> AddChecklistItemAsync(ChecklistItem checklistItem);
    Task<ChecklistItem> UpdateChecklistItemAsync(ChecklistItem checklistItem);
    Task DeleteChecklistItemAsync(Guid checklistItemId);
    Task<bool> ChecklistItemExistsAsync(Guid checklistItemId);
}
