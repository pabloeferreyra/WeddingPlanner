namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
public enum TaskPriority
{
    [Display(Name = "Baja")]
    Low,
    [Display(Name = "Media")]
    Medium,
    [Display(Name = "Alta")]
    High,
    [Display(Name = "Crítica")]
    Critical
}