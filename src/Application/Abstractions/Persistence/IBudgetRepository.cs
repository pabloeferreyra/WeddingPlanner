using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Persistence;
public interface IBudgetRepository
{
    Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default);
}