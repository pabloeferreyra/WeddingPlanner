namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
public enum WeddingStatus
{

    [Display(Name = "En planificación")]
    Planning = 1,
    [Display(Name = "Confirmado")]
    Confirmed = 2,
    [Display(Name = "Completado")]
    Completed = 3,
    [Display(Name = "Cancelado")]
    Cancelled = 4
}