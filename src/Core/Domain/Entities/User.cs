using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Core.Domain.Entities;

public class User : IdentityUser
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Wedding> Weddings { get; set; } = [];
}