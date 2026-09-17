namespace WeddingPlanner.Core.Domain.Entities;

public class Vendor
{
    public int Id { get; set; }
    public string BussinessName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<VendorDocument> VendorDocuments { get; set; } = [];
}