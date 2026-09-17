using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Wedding
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int GuestCount { get; set; } = 0;
    public decimal Budget { get; set; } = 0;
    public WeddingStatus Status { get; set; } = WeddingStatus.Planning;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;    
    public string UserId { get; set; } = string.Empty;
    public ICollection<Timeline> Timelines { get; set; } = [];
    public ICollection<Guest> Guests { get; set; } = [];
    public User? User { get; set; }
}