using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Entities;

public class Person
{
    [Required]
    public string? Name { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
}
