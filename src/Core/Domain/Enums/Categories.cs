namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
public enum TaskCategory
{

    [Display(Name = "Lugar")]
    Venue,
    [Display(Name = "Catering")]
    Catering,
    [Display(Name = "Invitaciones")]
    Invitations,
    [Display(Name = "Compras")]
    Shopping,
    [Display(Name = "Decoración")]
    DecorationsSetup,
    [Display(Name = "Gestión de invitados")]
    GuestManagement,
    [Display(Name = "Administrativo")]
    Administrative,
    [Display(Name = "Fotografía")]
    Photography,
    [Display(Name = "Música")]
    Music,
    [Display(Name = "Otro")]
    Other
}

public enum ExpenseCategory
{
    [Display(Name = "Catering")]
    Catering,
    [Display(Name = "Lugar")]
    Venue,
    [Display(Name = "Fotografía")]
    Photography,
    [Display(Name = "Videografía")]
    Videography,
    [Display(Name = "Flores")]
    Flowers,
    [Display(Name = "Decoración")]
    Decoration,
    [Display(Name = "Música")]
    Music,
    [Display(Name = "Pastel")]
    Cake,
    [Display(Name = "Transporte")]
    Transportation,
    [Display(Name = "Alojamiento")]
    Accommodations,
    [Display(Name = "Peinado y maquillaje")]
    HairMakeup,
    [Display(Name = "Invitaciones")]
    Invitations,
    [Display(Name = "Recuerdos")]
    Favors,
    [Display(Name = "Varios")]
    Miscellaneous
}

public enum TimelineCategory
{
    [Display(Name = "Antes de la evento")]
    PreWedding,
    [Display(Name = "Día del evento")]
    DayOfEvent,
    [Display(Name = "Después de la evento")] 
    PostWedding
}