namespace WeddingPlanner.Core.Domain.Enums;

using System.ComponentModel.DataAnnotations;

public enum GuestStatus
{
    [Display(Name = "Pendiente")]
    Pending = 1,
    [Display(Name = "Confirmado")]
    Confirmed = 2,
    [Display(Name = "Declinado")]
    Declined = 3
}