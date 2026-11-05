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
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly WeddingDate { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public Address? Location { get; set; }

    public int ExpectedGuestCount { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal InitialBudget { get; set; }

    public WeddingStatus Status { get; set; } = WeddingStatus.Planning;

    // Used when IsWedding is true
    public Person? Bride { get; set; }
    public Person? Groom { get; set; }

    // Used when IsWedding is false
    public Person? Celebrated { get; set; }

    // Coordinator (Foreign Key)
    public string? CoordinatorId { get; set; }
    public User? Coordinator { get; set; }

    // Timeline
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    public ICollection<Guest> Guests { get; set; } = [];
    public ICollection<VendorAssignment> VendorAssignments { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
    public ICollection<Timeline> Timelines { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public bool IsWedding {get; set; } = true;
    
}