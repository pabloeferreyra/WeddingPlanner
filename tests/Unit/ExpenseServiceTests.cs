using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Implementations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Tests.Unit;

public class ExpenseServiceTests
{
    [Fact]
    public void Expense_is_overdue_only_after_its_due_date_when_unpaid()
    {
        var expense = new Expense
        {
            Description = "Proveedor",
            ExpenseDate = new DateTime(2026, 11, 1),
            DueDate = new DateTime(2026, 11, 25),
            Amount = 100
        };

        Assert.Equal(
            ExpensePaymentStatus.Pending,
            expense.GetPaymentStatus(new DateOnly(2026, 11, 25)));
        Assert.Equal(
            ExpensePaymentStatus.Overdue,
            expense.GetPaymentStatus(new DateOnly(2026, 11, 26)));
    }

    [Fact]
    public void Payment_date_marks_expense_as_paid_even_if_due_date_passed()
    {
        var expense = new Expense
        {
            Description = "Proveedor",
            ExpenseDate = new DateTime(2026, 11, 1),
            DueDate = new DateTime(2026, 11, 20),
            PaymentDate = new DateTime(2026, 11, 26),
            Amount = 100
        };

        Assert.Equal(
            ExpensePaymentStatus.Paid,
            expense.GetPaymentStatus(new DateOnly(2026, 11, 27)));
    }

    [Fact]
    public async Task Expense_without_due_date_can_be_saved_and_remains_pending()
    {
        var repository = new StubExpenseRepository();
        var service = new ExpenseService(repository);
        var expense = new Expense
        {
            BudgetId = Guid.NewGuid(),
            Description = "Proveedor",
            ExpenseDate = DateTime.Today,
            Amount = 100,
            Category = ExpenseCategory.Catering
        };

        await service.AddExpenseAsync(expense);

        Assert.Same(expense, repository.AddedExpense);
        Assert.Equal(ExpensePaymentStatus.Pending, expense.GetPaymentStatus(DateOnly.FromDateTime(DateTime.Today)));
    }

    [Fact]
    public async Task AddExpense_rejects_non_positive_amount_before_persisting()
    {
        var repository = new StubExpenseRepository();
        var service = new ExpenseService(repository);

        await Assert.ThrowsAsync<ValidationException>(() => service.AddExpenseAsync(new Expense
        {
            Description = "Proveedor",
            ExpenseDate = DateTime.Today,
            Amount = 0,
            Category = ExpenseCategory.Catering
        }));

        Assert.Null(repository.AddedExpense);
    }

    private sealed class StubExpenseRepository : IRepository<Expense>
    {
        public Expense? AddedExpense { get; private set; }

        public Task<Expense> AddAsync(Expense entity)
        {
            AddedExpense = entity;
            return Task.FromResult(entity);
        }

        public Task<Expense> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IEnumerable<Expense>> GetAllAsync() => throw new NotSupportedException();
        public Task<Expense> UpdateAsync(Expense entity) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(Guid id) => throw new NotSupportedException();
        public IQueryable<Expense> AsQueryable() => throw new NotSupportedException();
    }
}
