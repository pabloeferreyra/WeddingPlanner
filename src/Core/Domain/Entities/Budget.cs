using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public Wedding? Wedding { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public decimal CurrentSpent { get; set; } = 0;

    // Budget by Categories
    public Dictionary<string, decimal> CategoryAllocations { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Expense> Expenses { get; set; } = [];

    // Methods
    public decimal GetRemainingBudget() => TotalAmount - CurrentSpent;
    public decimal GetPercentageSpent() => (CurrentSpent / TotalAmount) * 100;
    public bool IsOverBudget() => CurrentSpent > TotalAmount;
}