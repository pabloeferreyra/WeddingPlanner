using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Enums;

public enum VendorPaymentType
{
    [Display(Name = "Anticipo")]
    Deposit,
    [Display(Name = "Pago parcial")]
    Partial,
    [Display(Name = "Pago final")]
    Final
}
