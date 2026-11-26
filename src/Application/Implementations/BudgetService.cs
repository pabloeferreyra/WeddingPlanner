using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class BudgetService (IBudgetRepository budgetRepository, IRepository<Budget> Repository, ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default)
    {
        var budget = await budgetRepository.GetBudgetByWedding(weddingId, cancellationToken);
        if (budget is not null)
        {
            budget.CurrentSpent = await budgetRepository.GetTotalExpensesByBudget(budget.Id, cancellationToken);
        }

        return budget;
    }
    
    public async Task AddBudget(Budget budget)
    {
        budget.CurrentSpent = await budgetRepository.GetTotalExpensesByBudget(budget.Id);
        await Repository.AddAsync(budget);
    }
    
    public async Task UpdateBudget(Budget budget)
    {
        budget.CurrentSpent = await budgetRepository.GetTotalExpensesByBudget(budget.Id);
        await Repository.UpdateAsync(budget);
    }
    
    public async Task DeleteBudget(Guid budgetId)
    {
        var budget = await Repository.GetByIdAsync(budgetId);
        if (budget != null)
        {
            await Repository.DeleteAsync(budgetId);
        }
    }
    
    public async Task<IEnumerable<Budget>> GetAllBudgets()
    {
        return await Repository.GetAllAsync();
    }

    public async Task<Budget?> GetBudgetById(Guid budgetId)
    {
        return await Repository.GetByIdAsync(budgetId);
    }
}