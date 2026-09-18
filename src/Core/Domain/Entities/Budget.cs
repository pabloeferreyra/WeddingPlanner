using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal TotalAmount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public ICollection<Expense> Expenses { get; set; } = [];
}