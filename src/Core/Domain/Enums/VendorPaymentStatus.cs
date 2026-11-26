using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Core.Domain.Enums;

public enum VendorPaymentStatus
{
    [Display(Name = "Pendiente")]
    Pending,
    [Display(Name = "Pagado")]
    Paid,
    [Display(Name = "Vencido")]
    Overdue
}
