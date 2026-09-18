using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Guest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public required Wedding Wedding { get; set; }

    [Required(ErrorMessage = "Guest first name is required")]
    [StringLength(100)]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Guest last name is required")]
    [StringLength(100)]
    public required string LastName { get; set; }

    [EmailAddress]
    public required string Email { get; set; }

    public PhoneNumber? Phone { get; set; }

    public GuestStatus RSVPStatus { get; set; } = GuestStatus.Pending;

    public MealPreference MealPreference { get; set; } = MealPreference.Regular;

    public int? TableNumber { get; set; }

    [StringLength(500)]
    public required string Notes { get; set; }

    public bool IsFamily { get; set; }
    public bool IsVIP { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RSVPDate { get; set; }
}