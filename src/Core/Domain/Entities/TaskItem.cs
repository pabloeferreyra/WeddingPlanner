using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;
using TaskStatus = WeddingPlanner.Core.Domain.Enums.TaskStatus;
namespace WeddingPlanner.Core.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public Wedding? Wedding { get; set; }

    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public string? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskCategory Category { get; set; }

    public int EstimatedHours { get; set; } = 1;
    public int? ActualHours { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Checklist items
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
}