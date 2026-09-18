using Microsoft.AspNetCore.Identity;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Core.Domain.Entities;

public class User : IdentityUser
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Admin;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Wedding> Weddings { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
}