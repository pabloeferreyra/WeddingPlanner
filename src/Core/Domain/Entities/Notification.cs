using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required string UserId { get; set; }
    public required User User { get; set; }

    public Guid? WeddingId { get; set; }
    public required Wedding Wedding { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [StringLength(500)]
    public required string Message { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}