using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;
public class VendorDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; }

    public Guid? VendorAssignmentId { get; set; }
    public VendorAssignment VendorAssignment { get; set; }

    [Required(ErrorMessage = "Document name is required")]
    [StringLength(200)]
    public string DocumentName { get; set; }

    public DocumentType DocumentType { get; set; }

    [Url]
    [Required]
    public string? FileUrl { get; set; }

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiryDate { get; set; }
}