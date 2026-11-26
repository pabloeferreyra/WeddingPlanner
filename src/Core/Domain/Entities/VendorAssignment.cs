using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;
public class VendorAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public Wedding? Wedding { get; set; }

    [Required]
    public Guid VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public VendorServiceType ServiceType { get; set; }

    public DateTime ContractDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PaymentAmount { get; set; }

    [Range(0, 100)]
    public decimal DepositPercentage { get; set; } = 50;

    public decimal OpeningBalancePaid { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<VendorDocument> Contracts { get; set; } = [];
    public ICollection<VendorPayment> Payments { get; set; } = [];

    public PaymentStatus GetPaymentStatus(decimal totalPaid, bool hasOverduePayments) =>
        PaymentAmount > 0 && totalPaid >= PaymentAmount
            ? PaymentStatus.Completed
            : hasOverduePayments
                ? PaymentStatus.Overdue
                : PaymentStatus.Pending;
}