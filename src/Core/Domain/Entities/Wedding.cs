using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Wedding
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Wedding title is required")]
    [StringLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime WeddingDate { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public Address? Location { get; set; }

    public int ExpectedGuestCount { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal InitialBudget { get; set; }

    public WeddingStatus Status { get; set; } = WeddingStatus.Planning;

    // Bride & Groom
    [Required]
    public string? BrideName { get; set; }

    [Required]
    public string? GroomName { get; set; }

    public string? BrideEmail { get; set; }
    public string? GroomEmail { get; set; }
    public string? BridePhone { get; set; }
    public string? GroomPhone { get; set; }

    // Coordinator (Foreign Key)
    public string? CoordinatorId { get; set; }
    public User? Coordinator { get; set; }

    // Timeline
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public ICollection<Guest> Guests { get; set; } = [];
    public ICollection<VendorAssignment> VendorAssignments { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
    public ICollection<Timeline> Timelines { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
}