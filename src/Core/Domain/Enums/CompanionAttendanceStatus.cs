namespace WeddingPlanner.Core.Domain.Enums;

using System.ComponentModel.DataAnnotations;

public enum CompanionAttendanceStatus
{
    [Display(Name = "Pendiente")]
    Pending = 0,
    [Display(Name = "Asistirá acompañado")]
    Confirmed = 1,
    [Display(Name = "No asistirá acompañado")]
    Declined = 2
}
