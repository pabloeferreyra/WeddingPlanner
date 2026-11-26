using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class VendorPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VendorAssignmentId { get; set; }
    public VendorAssignment? VendorAssignment { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public VendorPaymentType Type { get; set; } = VendorPaymentType.Partial;
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }

    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    [Url]
    public string? ReceiptUrl { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    public VendorPaymentStatus GetStatus(DateOnly asOfDate)
    {
        if (PaymentDate.HasValue)
        {
            return VendorPaymentStatus.Paid;
        }

        return DueDate.HasValue && DateOnly.FromDateTime(DueDate.Value) < asOfDate
            ? VendorPaymentStatus.Overdue
            : VendorPaymentStatus.Pending;
    }
}
