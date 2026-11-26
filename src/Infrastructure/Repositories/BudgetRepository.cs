using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public class BudgetRepository(ApplicationDbContext context, 
ILogger<BudgetRepository> logger) : IBudgetRepository
{
    public async Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting budget for wedding {WeddingId}", weddingId);

        var budget = await context.Budgets.FirstOrDefaultAsync(b => b.WeddingId == weddingId, cancellationToken);

        if (budget is null)
        {
            logger.LogWarning("No budget found for wedding {WeddingId}", weddingId);
        }

        return budget;
    }

    public async Task<decimal> GetTotalExpensesByBudget(Guid budgetId, CancellationToken cancellationToken = default)
    {
        var total = await context.Expenses
            .Where(expense => expense.BudgetId == budgetId)
            .SumAsync(expense => (decimal?)expense.Amount, cancellationToken);

        return total ?? 0m;
    }
}