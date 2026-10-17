using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetAllExpensesAsync();
    Task<Expense> GetExpenseByIdAsync(Guid expenseId);
    Task<Expense> AddExpenseAsync(Expense expense);
    Task<Expense> UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(Guid expenseId);
    Task<bool> ExpenseExistsAsync(Guid expenseId);
}
