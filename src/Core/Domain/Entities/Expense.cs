using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int BudgetId { get; set; }
    public Budget Budget { get; set; } = new Budget();
    public int CategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Catering;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public Vendor? Vendor { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public bool IsRecurring { get; set; } = false;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}