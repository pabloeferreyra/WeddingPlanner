using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Enums;

public enum VendorServiceType
{
    [Display(Name = "Catering")]
    Catering,
    [Display(Name = "Salón o lugar")]
    Venue,
    [Display(Name = "Fotografía")]
    Photography,
    [Display(Name = "Video")]
    Videography,
    [Display(Name = "Florería")]
    Florist,
    [Display(Name = "Decoración")]
    Decoration,
    [Display(Name = "DJ")]
    DJs,
    [Display(Name = "Pastelería")]
    Cake,
    [Display(Name = "Invitaciones")]
    Invitations,
    [Display(Name = "Transporte")]
    Transportation,
    [Display(Name = "Peluquería y maquillaje")]
    HairMakeup,
    [Display(Name = "Entretenimiento")]
    Entertainment,
    [Display(Name = "Otro")]
    Other
}