using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Tests.Unit;

public class BudgetServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 125)]
    public async Task GetBudgetByWedding_uses_sum_of_expenses(decimal storedSpent, decimal expensesTotal)
    {
        var weddingId = Guid.NewGuid();
        var budget = new Budget
        {
            WeddingId = weddingId,
            TotalAmount = 400,
            CurrentSpent = storedSpent
        };
        var service = new BudgetService(
            new StubBudgetRepository(budget, expensesTotal),
            new StubRepository(),
            NullLogger<BudgetService>.Instance);

        var result = await service.GetBudgetByWedding(weddingId);

        Assert.NotNull(result);
        Assert.Equal(expensesTotal, result.CurrentSpent);
        Assert.Equal(budget.TotalAmount - expensesTotal, result.GetRemainingBudget());
    }

    private sealed class StubBudgetRepository(Budget budget, decimal expensesTotal) : IBudgetRepository
    {
        public Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Budget?>(budget.WeddingId == weddingId ? budget : null);

        public Task<decimal> GetTotalExpensesByBudget(Guid budgetId, CancellationToken cancellationToken = default) =>
            Task.FromResult(expensesTotal);
    }

    private sealed class StubRepository : IRepository<Budget>
    {
        public Task<Budget> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IEnumerable<Budget>> GetAllAsync() => throw new NotSupportedException();
        public Task<Budget> AddAsync(Budget entity) => throw new NotSupportedException();
        public Task<Budget> UpdateAsync(Budget entity) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(Guid id) => throw new NotSupportedException();
        public IQueryable<Budget> AsQueryable() => throw new NotSupportedException();
    }
}
