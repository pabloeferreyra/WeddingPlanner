using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;
public class VendorDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public Guid? VendorAssignmentId { get; set; }
    public VendorAssignment? VendorAssignment { get; set; }

    [Required(ErrorMessage = "Document name is required")]
    [StringLength(200)]
    public string? DocumentName { get; set; }

    public DocumentType DocumentType { get; set; }

    [Url]
    public string? FileUrl { get; set; }

    [StringLength(500)]
    public string? StorageKey { get; set; }
    [StringLength(255)]
    public string? FileName { get; set; }
    [StringLength(100)]
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    [StringLength(1000)]
    public string? Description { get; set; }
    public bool DeletionPending { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime? ExpiryDate { get; set; }
}