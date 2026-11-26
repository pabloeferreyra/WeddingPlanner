namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

public enum PaymentStatus
{
    [Display(Name = "Pendiente")]
    Pending = 1,
    [Display(Name = "Completado")]
    Completed = 2,
    [Display(Name = "Fallido")]
    Failed = 3
}