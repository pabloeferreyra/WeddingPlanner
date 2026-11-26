using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

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
        ValidateBudget(budget);
        budget.CurrentSpent = await budgetRepository.GetTotalExpensesByBudget(budget.Id);
        await Repository.AddAsync(budget);
    }
    
    public async Task UpdateBudget(Budget budget)
    {
        ValidateBudget(budget);
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

    private static void ValidateBudget(Budget budget)
    {
        if (budget.TotalAmount <= 0)
        {
            throw new ValidationException("El presupuesto total debe ser mayor que cero.");
        }

        if (budget.CategoryAllocations is null)
        {
            throw new ValidationException("Las asignaciones por categoría no son válidas.");
        }

        foreach (var allocation in budget.CategoryAllocations)
        {
            if (!Enum.TryParse<ExpenseCategory>(allocation.Key, out var category)
                || !string.Equals(Enum.GetName(category), allocation.Key, StringComparison.Ordinal)
                || !Enum.IsDefined(category))
            {
                throw new ValidationException("La asignación contiene una categoría inválida.");
            }

            if (allocation.Value < 0)
            {
                throw new ValidationException("Las asignaciones por categoría no pueden ser negativas.");
            }
        }
    }
}