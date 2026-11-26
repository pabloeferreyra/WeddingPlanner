namespace WeddingPlanner.Core.Domain.Enums;

using System.ComponentModel.DataAnnotations;

public enum MealPreference
{
    [Display(Name = "Regular")]
    Regular,
    [Display(Name = "Vegetariana")]
    Vegetarian,
    [Display(Name = "Vegana")]
    Vegan,
    [Display(Name = "Sin gluten")]
    GlutenFree,
    [Display(Name = "Sin preferencia")]
    NoPreference
}