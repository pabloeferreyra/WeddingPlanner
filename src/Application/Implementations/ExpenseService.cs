using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Implementations;

public sealed class ExpenseService(IRepository<Expense> repository) : IExpenseService
{
    public async Task<IEnumerable<Expense>> GetAllExpensesAsync() => await repository.GetAllAsync();
    public async Task<Expense> GetExpenseByIdAsync(Guid expenseId) => await repository.GetByIdAsync(expenseId);
    public async Task<Expense> AddExpenseAsync(Expense expense)
    {
        ValidateExpense(expense);
        return await repository.AddAsync(expense);
    }

    public async Task<Expense> UpdateExpenseAsync(Expense expense)
    {
        ValidateExpense(expense);
        return await repository.UpdateAsync(expense);
    }

    public async Task DeleteExpenseAsync(Guid expenseId) => await repository.DeleteAsync(expenseId);
    public async Task<bool> ExpenseExistsAsync(Guid expenseId) => await repository.ExistsAsync(expenseId);

    private static void ValidateExpense(Expense expense)
    {
        if (expense.Amount <= 0)
        {
            throw new ValidationException("El importe del gasto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(expense.Description))
        {
            throw new ValidationException("La descripción del gasto es obligatoria.");
        }

        if (!Enum.IsDefined(expense.Category))
        {
            throw new ValidationException("Selecciona una categoría válida.");
        }

        if (expense.ExpenseDate == default)
        {
            throw new ValidationException("La fecha del gasto es obligatoria.");
        }

        if ((expense.DueDate.HasValue && expense.DueDate.Value == default)
            || (expense.PaymentDate.HasValue && expense.PaymentDate.Value == default))
        {
            throw new ValidationException("Las fechas del gasto no son válidas.");
        }

        if (expense.IsRecurring && expense.RecurrenceType is null)
        {
            throw new ValidationException("Selecciona la frecuencia del gasto recurrente.");
        }

        if (expense.RecurrenceType.HasValue && !Enum.IsDefined(expense.RecurrenceType.Value))
        {
            throw new ValidationException("Selecciona una frecuencia válida.");
        }

        if (!expense.IsRecurring)
        {
            expense.RecurrenceType = null;
        }
    }
}
