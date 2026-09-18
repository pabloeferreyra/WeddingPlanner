using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Timeline
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public required Wedding Wedding { get; set; }

    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200)]
    public required string EventName { get; set; }

    [StringLength(500)]
    public required string Description { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    public int DurationMinutes { get; set; } = 60;

    [StringLength(200)]
    public required string Location { get; set; }

    public required string AssignedToId { get; set; }
    public required User AssignedTo { get; set; }

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public required TimelineCategory Category { get; set; }

    [StringLength(500)]
    public required string Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}