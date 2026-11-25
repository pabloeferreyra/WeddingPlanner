using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Web.Models;

public sealed class WeddingFormViewModel : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsWedding)
        {
            if (string.IsNullOrWhiteSpace(BrideName))
                yield return new ValidationResult("El nombre de la novia es obligatorio.", [nameof(BrideName)]);
            if (string.IsNullOrWhiteSpace(GroomName))
                yield return new ValidationResult("El nombre del novio es obligatorio.", [nameof(GroomName)]);
        }
        else if (string.IsNullOrWhiteSpace(CelebratedName))
        {
            yield return new ValidationResult("El nombre del festejado/a es obligatorio.", [nameof(CelebratedName)]);
        }
    }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de la evento")]
    public DateOnly WeddingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Es una boda")]
    public bool IsWedding { get; set; } = true;

    [EnumDataType(typeof(WeddingStatus), ErrorMessage = "El estado seleccionado no es válido.")]
    [Display(Name = "Estado")]
    public WeddingStatus Status { get; set; } = WeddingStatus.Planning;

    [StringLength(100)]
    [Display(Name = "Nombre de la novia")]
    public string BrideName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email de la novia")]
    public string? BrideEmail { get; set; }

    [Phone]
    [Display(Name = "Teléfono de la novia")]
    public string? BridePhone { get; set; }

    [StringLength(100)]
    [Display(Name = "Nombre del novio")]
    public string GroomName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email del novio")]
    public string? GroomEmail { get; set; }

    [Phone]
    [Display(Name = "Teléfono del novio")]
    public string? GroomPhone { get; set; }

    [StringLength(100)]
    [Display(Name = "Nombre del festejado/a")]
    public string CelebratedName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email del festejado/a")]
    public string? CelebratedEmail { get; set; }

    [Phone]
    [Display(Name = "Teléfono del festejado/a")]
    public string? CelebratedPhone { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(200)]
    [Display(Name = "Calle y número")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Ciudad")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "La provincia o estado es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Provincia / Estado")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código postal es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Código postal")]
    public string ZipCode { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad de invitados no puede ser negativa.")]
    [Display(Name = "Invitados esperados")]
    public int ExpectedGuestCount { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El presupuesto debe ser mayor que cero.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Presupuesto inicial")]
    public decimal InitialBudget { get; set; } = 0.01m;
}
