using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Entities;

public class WeddingTable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid WeddingId { get; set; }
    public Wedding? Wedding { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public ICollection<Guest> Guests { get; set; } = [];
}
