using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Budgets;

public sealed record GetBudgetByWeddingQuery(Guid WeddingId);

public sealed class GetBudgetByWeddingHandler(IBudgetService service)
{
    public Task<Budget?> HandleAsync(GetBudgetByWeddingQuery query, CancellationToken cancellationToken = default) =>
        service.GetBudgetByWedding(query.WeddingId, cancellationToken);
}

public sealed record AddBudgetCommand(Budget Budget);

public sealed class AddBudgetHandler(IBudgetService service)
{
    public Task HandleAsync(AddBudgetCommand command) => service.AddBudget(command.Budget);
}

public sealed record UpdateBudgetCommand(Budget Budget);

public sealed class UpdateBudgetHandler(IBudgetService service)
{
    public Task HandleAsync(UpdateBudgetCommand command) => service.UpdateBudget(command.Budget);
}

public sealed record DeleteBudgetCommand(Guid BudgetId);

public sealed class DeleteBudgetHandler(IBudgetService service)
{
    public Task HandleAsync(DeleteBudgetCommand command) => service.DeleteBudget(command.BudgetId);
}

public sealed class GetAllBudgetsHandler(IBudgetService service)
{
    public Task<IEnumerable<Budget>> HandleAsync() => service.GetAllBudgets();
}

public sealed record GetBudgetByIdQuery(Guid BudgetId);

public sealed class GetBudgetByIdHandler(IBudgetService service)
{
    public Task<Budget?> HandleAsync(GetBudgetByIdQuery query) => service.GetBudgetById(query.BudgetId);
}
