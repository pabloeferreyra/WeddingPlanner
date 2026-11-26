namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
public enum TaskStatus
{
    [Display(Name = "Por hacer")]
    Todo,
    [Display(Name = "En progreso")]
    InProgress,
    [Display(Name = "En revisión")]
    InReview,
    [Display(Name = "Completado")]
    Completed,
    [Display(Name = "Bloqueado")]
    Blocked
}