using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Web.Models;

public class VendorFormViewModel
{
    [Required(ErrorMessage = "El nombre comercial es obligatorio.")]
    [StringLength(200)]
    public string BusinessName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string? Email { get; set; }
    public PhoneNumber? Phone { get; set; } = new();

    [Url(ErrorMessage = "Ingresa una URL válida.")]
    public string? Website { get; set; }
    public VendorServiceType ServiceType { get; set; }
    public Address Address { get; set; } = new()
    {
        Street = string.Empty,
        City = string.Empty,
        State = string.Empty,
        ZipCode = string.Empty
    };
    public string? Description { get; set; }

    public decimal AverageRating { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFavorite { get; set; } = false;
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
}