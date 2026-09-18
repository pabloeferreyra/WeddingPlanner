using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TaskId { get; set; }
    public TaskItem? Task { get; set; }

    [Required]
    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}