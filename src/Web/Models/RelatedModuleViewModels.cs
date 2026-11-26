using System.ComponentModel.DataAnnotations;
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

    public int ConfirmedMainGuestCount => Guests.Count(guest =>
        guest.RSVPStatus == GuestStatus.Confirmed);

    public int ConfirmedCompanionCount => Guests.Count(guest =>
        guest.CanBringCompanion && guest.CompanionStatus == CompanionAttendanceStatus.Confirmed);

    public int ConfirmedAttendeeCount => ConfirmedMainGuestCount + ConfirmedCompanionCount;
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

    [Range(1, int.MaxValue)]
    public int? TableNumber { get; set; }

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

public sealed class BudgetFormViewModel
{
    public Guid Id { get; set; }
    public Guid WeddingId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
}

public sealed class ExpenseFormViewModel
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
    public DateTime? PaymentDate { get; set; }

    public bool IsRecurring { get; set; }

    [EnumDataType(typeof(RecurrenceType))]
    public RecurrenceType? RecurrenceType { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Url]
    public string? ReceiptUrl { get; set; }
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

    [Range(0, double.MaxValue)]
    public decimal DepositPaid { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalPaid { get; set; }

    [EnumDataType(typeof(PaymentStatus))]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed class VendorDocumentListViewModel
{
    public required Vendor Vendor { get; init; }
    public IReadOnlyList<VendorDocument> Documents { get; init; } = [];
    public IReadOnlyList<VendorAssignment> Assignments { get; init; } = [];
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

    [Required, Url]
    public string FileUrl { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }
}
