namespace WeddingPlanner.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
public enum DocumentType
{
    [Display(Name = "Contrato")]
    Contract,
    [Display(Name = "Factura")]
    Invoice,
    [Display(Name = "Recibo")]
    Receipt,
    [Display(Name = "Licencia")]
    License,
    [Display(Name = "Seguro")]
    Insurance,
    [Display(Name = "Acuerdo")]
    Agreement,
    [Display(Name = "Otro")]
    Other
}