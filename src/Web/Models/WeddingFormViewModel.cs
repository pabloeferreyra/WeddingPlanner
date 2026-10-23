using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Web.Models;

public sealed class WeddingFormViewModel
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de la boda")]
    public DateTime WeddingDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El nombre de la novia es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre de la novia")]
    public string BrideName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del novio es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre del novio")]
    public string GroomName { get; set; } = string.Empty;

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
