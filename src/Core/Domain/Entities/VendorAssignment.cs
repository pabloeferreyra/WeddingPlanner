namespace WeddingPlanner.Core.Domain.Entities;
public class VendorAssignment
{
    public int Id { get; set; }
    public int WeddingId { get; set; }
    public int VendorId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ContractDate { get; set; } = DateTime.Now;
    public decimal PaymentAmount { get; set; } = 0;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string Notes { get; set; } = string.Empty;
    public string AssignedAt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Wedding? Wedding { get; set; }
    public Vendor? Vendor { get; set; }
}