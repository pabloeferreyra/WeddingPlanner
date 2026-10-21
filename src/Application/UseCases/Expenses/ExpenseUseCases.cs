using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Expenses;

public sealed class GetAllExpensesHandler(IExpenseService service)
{
    public Task<IEnumerable<Expense>> HandleAsync() => service.GetAllExpensesAsync();
}

public sealed record GetExpenseByIdQuery(Guid ExpenseId);

public sealed class GetExpenseByIdHandler(IExpenseService service)
{
    public Task<Expense> HandleAsync(GetExpenseByIdQuery query) => service.GetExpenseByIdAsync(query.ExpenseId);
}

public sealed record AddExpenseCommand(Expense Expense);

public sealed class AddExpenseHandler(IExpenseService service)
{
    public Task<Expense> HandleAsync(AddExpenseCommand command) => service.AddExpenseAsync(command.Expense);
}

public sealed record UpdateExpenseCommand(Expense Expense);

public sealed class UpdateExpenseHandler(IExpenseService service)
{
    public Task<Expense> HandleAsync(UpdateExpenseCommand command) => service.UpdateExpenseAsync(command.Expense);
}

public sealed record DeleteExpenseCommand(Guid ExpenseId);

public sealed class DeleteExpenseHandler(IExpenseService service)
{
    public Task HandleAsync(DeleteExpenseCommand command) => service.DeleteExpenseAsync(command.ExpenseId);
}

public sealed record ExpenseExistsQuery(Guid ExpenseId);

public sealed class ExpenseExistsHandler(IExpenseService service)
{
    public Task<bool> HandleAsync(ExpenseExistsQuery query) => service.ExpenseExistsAsync(query.ExpenseId);
}
