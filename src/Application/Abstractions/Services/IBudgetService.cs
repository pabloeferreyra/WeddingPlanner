using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;
public interface IBudgetService
{
    Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default);
    Task AddBudget(Budget budget);
    Task UpdateBudget(Budget budget);
    Task DeleteBudget(Guid budgetId);
    Task<IEnumerable<Budget>> GetAllBudgets();
    Task<Budget?> GetBudgetById(Guid budgetId);
}