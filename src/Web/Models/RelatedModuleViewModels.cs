using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;
using TaskStatus = WeddingPlanner.Core.Domain.Enums.TaskStatus;

namespace WeddingPlanner.Web.Models;

public sealed class WeddingPlanningViewModel
{
    public required Wedding Wedding { get; init; }
    public Budget? Budget { get; init; }
    public IReadOnlyList<Guest> Guests { get; init; } = [];
    public IReadOnlyList<Expense> Expenses { get; init; } = [];
    public IReadOnlyList<TaskItem> Tasks { get; init; } = [];
    public IReadOnlyList<ChecklistItem> ChecklistItems { get; init; } = [];
    public IReadOnlyList<Timeline> Timelines { get; init; } = [];
    public IReadOnlyList<VendorAssignment> VendorAssignments { get; init; } = [];
    public IReadOnlyList<Vendor> Vendors { get; init; } = [];
    public IReadOnlyList<VendorPayment> VendorPayments { get; init; } = [];
    public WeddingSeatingSummary Seating { get; init; } = new([], []);

    public int ConfirmedMainGuestCount { get; init; }
    public int ConfirmedCompanionCount { get; init; }
    public int ConfirmedAttendeeCount { get; init; }

    public IReadOnlyList<BudgetCategorySummary> BudgetCategorySummaries
    {
        get
        {
            if (Budget is null)
            {
                return [];
            }

            var expensesByCategory = Expenses
                .GroupBy(expense => expense.Category)
                .ToDictionary(group => group.Key, group => group.Sum(expense => expense.Amount));

            return Enum.GetValues<ExpenseCategory>()
                .Select(category => new BudgetCategorySummary(
                    category,
                    Budget.CategoryAllocations.GetValueOrDefault(category.ToString()),
                    expensesByCategory.GetValueOrDefault(category)))
                .Where(summary => summary.AllocatedAmount != 0 || summary.ActualSpent != 0)
                .ToList();
        }
    }

    public IReadOnlyList<VendorAssignmentPaymentSummary> VendorAssignmentPaymentSummaries
        {
            get
            {
                var paymentsByAssignment = VendorPayments
                    .GroupBy(payment => payment.VendorAssignmentId)
                    .ToDictionary(group => group.Key, group => group.ToList());
                var asOfDate = DateOnly.FromDateTime(DateTime.Today);

                return VendorAssignments.Select(assignment =>
                {
                    var payments = paymentsByAssignment.GetValueOrDefault(assignment.Id) ?? [];
                    var totalPaid = assignment.OpeningBalancePaid
                        + payments.Where(payment => payment.PaymentDate.HasValue).Sum(payment => payment.Amount);
                    var totalCommitted = assignment.OpeningBalancePaid
                        + payments.Sum(payment => payment.Amount);
                    var hasOverduePayments = payments.Any(payment =>
                        payment.GetStatus(asOfDate) == VendorPaymentStatus.Overdue);

                    return new VendorAssignmentPaymentSummary(
                        assignment,
                        totalPaid,
                        totalCommitted,
                        assignment.GetPaymentStatus(totalPaid, hasOverduePayments));
                }).ToList();
            }
    }
}

public sealed record BudgetCategorySummary(
    ExpenseCategory Category,
    decimal AllocatedAmount,
    decimal ActualSpent);

public sealed record VendorAssignmentPaymentSummary(
    VendorAssignment Assignment,
    decimal TotalPaid,
    decimal TotalCommitted,
    PaymentStatus PaymentStatus)
{
    public decimal RemainingBalance => Assignment.PaymentAmount - TotalPaid;
    public decimal UnscheduledAmount => Assignment.PaymentAmount - TotalCommitted;
}

public sealed class GuestFormViewModel : IValidatableObject
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneAreaCode { get; set; }
    public string? PhoneNumber { get; set; }

    [EnumDataType(typeof(GuestStatus))]
    public GuestStatus RSVPStatus { get; set; } = GuestStatus.Pending;

    public bool CanBringCompanion { get; set; }

    [EnumDataType(typeof(CompanionAttendanceStatus))]
    public CompanionAttendanceStatus CompanionStatus { get; set; } = CompanionAttendanceStatus.Pending;

    [StringLength(100)]
    public string? CompanionName { get; set; }

    [EnumDataType(typeof(MealPreference))]
    public MealPreference? CompanionMealPreference { get; set; }

    [StringLength(500)]
    public string? CompanionDietaryRestrictions { get; set; }

    [StringLength(500)]
    public string? CompanionNotes { get; set; }

    [EnumDataType(typeof(MealPreference))]
    public MealPreference MealPreference { get; set; } = MealPreference.Regular;

    public Guid? WeddingTableId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public bool IsFamily { get; set; }
    public bool IsVIP { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!CanBringCompanion && CompanionStatus == CompanionAttendanceStatus.Confirmed)
        {
            yield return new ValidationResult(
                "No se puede confirmar un acompañante si el invitado no tiene permiso para llevarlo.",
                [nameof(CanBringCompanion)]);
        }
    }
}

public sealed class WeddingTableFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed class BudgetFormViewModel : IValidatableObject
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public Dictionary<string, decimal?> CategoryAllocations { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CategoryAllocations is null)
        {
            yield return new ValidationResult(
                "Las asignaciones por categoría no son válidas.",
                [nameof(CategoryAllocations)]);
            yield break;
        }

        foreach (var allocation in CategoryAllocations)
        {
            if (!Enum.TryParse<ExpenseCategory>(allocation.Key, out var category)
                || !string.Equals(Enum.GetName(category), allocation.Key, StringComparison.Ordinal)
                || !Enum.IsDefined(category))
            {
                yield return new ValidationResult(
                    "La asignación contiene una categoría inválida.",
                    [nameof(CategoryAllocations)]);
                yield break;
            }

            if (allocation.Value < 0)
            {
                yield return new ValidationResult(
                    "Las asignaciones por categoría no pueden ser negativas.",
                    [nameof(CategoryAllocations)]);
            }
        }
    }
}

public sealed class ExpenseFormViewModel : IValidatableObject
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }
    public Guid BudgetId { get; set; }

    [EnumDataType(typeof(ExpenseCategory))]
    public ExpenseCategory Category { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? VendorName { get; set; }

    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PaymentDate { get; set; }

    public bool IsRecurring { get; set; }

    [EnumDataType(typeof(RecurrenceType))]
    public RecurrenceType? RecurrenceType { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Url]
    public string? ReceiptUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsRecurring && RecurrenceType is null)
        {
            yield return new ValidationResult(
                "Selecciona la frecuencia del gasto recurrente.",
                [nameof(RecurrenceType)]);
        }
    }
}

public sealed class TaskItemFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today;

    [EnumDataType(typeof(TaskStatus))]
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    [EnumDataType(typeof(TaskPriority))]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [EnumDataType(typeof(TaskCategory))]
    public TaskCategory Category { get; set; }

    [Range(1, int.MaxValue)]
    public int EstimatedHours { get; set; } = 1;

    [Range(0, int.MaxValue)]
    public int? ActualHours { get; set; }
}

public sealed class ChecklistItemFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }
    public Guid TaskId { get; set; }

    [Required, StringLength(300)]
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}

public sealed class TimelineFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Required, StringLength(200)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime ScheduledDate { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue)]
    public int DurationMinutes { get; set; } = 60;

    [StringLength(200)]
    public string? Location { get; set; }

    public bool IsCompleted { get; set; }

    [EnumDataType(typeof(TimelineCategory))]
    public TimelineCategory Category { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed class VendorAssignmentFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Required]
    public Guid VendorId { get; set; }

    [DataType(DataType.Date)]
    public DateTime ContractDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? ContractEndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PaymentAmount { get; set; }

    [Range(0, 100)]
    public decimal DepositPercentage { get; set; } = 50;

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed class VendorPaymentFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }
    public Guid VendorAssignmentId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [EnumDataType(typeof(VendorPaymentType))]
    public VendorPaymentType Type { get; set; } = VendorPaymentType.Partial;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PaymentDate { get; set; }

    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    [Url]
    public string? ReceiptUrl { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed class VendorDocumentListViewModel
{
    public required Vendor Vendor { get; init; }
    public IReadOnlyList<VendorDocument> Documents { get; init; } = [];
    public IReadOnlyList<WeddingPlanner.Application.Abstractions.Persistence.VendorDocumentAssignment> Assignments { get; init; } = [];
}

public sealed class NotificationListViewModel
{
    public IReadOnlyList<Notification> Notifications { get; init; } = [];
}

public sealed class VendorDocumentFormViewModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? VendorAssignmentId { get; set; }

    [Required, StringLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [EnumDataType(typeof(DocumentType))]
    public DocumentType DocumentType { get; set; }

    [Url]
    public string? FileUrl { get; set; }

    public IFormFile? File { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }
}
