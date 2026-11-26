using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }

    [Required(ErrorMessage = "Expense category is required")]
    public ExpenseCategory Category { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? VendorName { get; set; }

    public DateTime ExpenseDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public string? ReceiptUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    public ExpensePaymentStatus GetPaymentStatus(DateOnly asOfDate)
    {
        if (PaymentDate.HasValue)
        {
            return ExpensePaymentStatus.Paid;
        }

        return DueDate.HasValue && DateOnly.FromDateTime(DueDate.Value) < asOfDate
            ? ExpensePaymentStatus.Overdue
            : ExpensePaymentStatus.Pending;
    }
}