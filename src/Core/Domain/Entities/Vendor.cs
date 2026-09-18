using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Vendor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Business name is required")]
    [StringLength(200)]
    public required string BusinessName { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [Required]
    public string? Email { get; set; }

    public PhoneNumber? Phone { get; set; }

    [Url]
    public string? Website { get; set; }

    [Required]
    public VendorServiceType ServiceType { get; set; }

    public required Address Address { get; set; }

    public string? Description { get; set; }

    public decimal AverageRating { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsFavorite { get; set; } = false;

    // Social Media
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VendorAssignment> Assignments { get; set; } = [];
    public ICollection<VendorDocument> Documents { get; set; } = [];
}