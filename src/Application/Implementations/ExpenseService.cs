using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class ExpenseService(IRepository<Expense> repository) : IExpenseService
{
    public async Task<IEnumerable<Expense>> GetAllExpensesAsync() => await repository.GetAllAsync();
    public async Task<Expense> GetExpenseByIdAsync(Guid expenseId) => await repository.GetByIdAsync(expenseId);
    public async Task<Expense> AddExpenseAsync(Expense expense) => await repository.AddAsync(expense);
    public async Task<Expense> UpdateExpenseAsync(Expense expense) => await repository.UpdateAsync(expense);
    public async Task DeleteExpenseAsync(Guid expenseId) => await repository.DeleteAsync(expenseId);
    public async Task<bool> ExpenseExistsAsync(Guid expenseId) => await repository.ExistsAsync(expenseId);
}
