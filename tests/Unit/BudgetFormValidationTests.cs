using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Tests.Unit;

public class BudgetFormValidationTests
{
    [Fact]
    public void Empty_category_allocations_are_allowed()
    {
        var model = new BudgetFormViewModel
        {
            TotalAmount = 100,
            CategoryAllocations = new Dictionary<string, decimal?> { ["Venue"] = null }
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        Assert.True(isValid);
    }

    [Fact]
    public void Negative_category_allocations_are_rejected()
    {
        var model = new BudgetFormViewModel
        {
            TotalAmount = 100,
            CategoryAllocations = new Dictionary<string, decimal?> { ["Venue"] = -1 }
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(BudgetFormViewModel.CategoryAllocations)));
    }

    [Fact]
    public void Category_summary_compares_allocated_budget_to_registered_expenses()
    {
        var viewModel = new WeddingPlanningViewModel
        {
            Wedding = new Wedding
            {
                Title = "Celebración",
                WeddingDate = new DateOnly(2027, 5, 1)
            },
            Budget = new Budget
            {
                TotalAmount = 1000,
                CategoryAllocations = new Dictionary<string, decimal> { ["Catering"] = 300 }
            },
            Expenses =
            [
                new Expense
                {
                    Description = "Seña",
                    ExpenseDate = new DateTime(2026, 11, 1),
                    Amount = 75,
                    Category = ExpenseCategory.Catering
                }
            ]
        };

        var summary = Assert.Single(viewModel.BudgetCategorySummaries);

        Assert.Equal(ExpenseCategory.Catering, summary.Category);
        Assert.Equal(300, summary.AllocatedAmount);
        Assert.Equal(75, summary.ActualSpent);
    }
}
