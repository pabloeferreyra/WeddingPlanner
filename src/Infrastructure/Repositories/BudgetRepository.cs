using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Infrastructure.Data;

namespace WeddingPlanner.Infrastructure.Repositories;

public class BudgetRepository(ApplicationDbContext context) : IBudgetRepository
{
    public async Task<Budget?> GetBudgetByWedding(Guid weddingId, CancellationToken cancellationToken = default)
    {
        return await context.Budgets.FirstOrDefaultAsync(b => b.WeddingId == weddingId, cancellationToken); 
    }
}