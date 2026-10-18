using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Timeline
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public Wedding? Wedding { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200)]
    public string? EventName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    public int DurationMinutes { get; set; } = 60;

    [StringLength(200)]
    public string? Location { get; set; }

    public string? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public TimelineCategory Category { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}