using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class Guest
{
    public int Id { get; set; }
    public Wedding Wedding { get; set; } = new Wedding();
    public int WeddingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public GuestStatus Status { get; set; } = GuestStatus.Pending;
    public string MealPreference { get; set; } = string.Empty;
    public int TableNumber { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}