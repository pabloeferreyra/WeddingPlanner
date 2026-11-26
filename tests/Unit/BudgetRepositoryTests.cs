using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;
using WeddingPlanner.Infrastructure.Repositories;

namespace WeddingPlanner.Tests.Unit;

public class BudgetRepositoryTests
{
    [Fact]
    public async Task GetTotalExpensesByBudget_sums_only_the_selected_budget_and_returns_zero_when_empty()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var budget = new Budget { WeddingId = Guid.NewGuid() };
        var otherBudget = new Budget { WeddingId = Guid.NewGuid() };
        var emptyBudget = new Budget { WeddingId = Guid.NewGuid() };

        await using var context = new ApplicationDbContext(options);
        context.Budgets.AddRange(budget, otherBudget, emptyBudget);
        context.Expenses.AddRange(
            new Expense { BudgetId = budget.Id, Amount = 45, Description = "Catering" },
            new Expense { BudgetId = budget.Id, Amount = 30, Description = "Flowers" },
            new Expense { BudgetId = otherBudget.Id, Amount = 900, Description = "Venue" });
        await context.SaveChangesAsync();

        var repository = new BudgetRepository(context, NullLogger<BudgetRepository>.Instance);

        Assert.Equal(75, await repository.GetTotalExpensesByBudget(budget.Id));
        Assert.Equal(0, await repository.GetTotalExpensesByBudget(emptyBudget.Id));
    }
}
