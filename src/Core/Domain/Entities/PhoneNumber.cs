namespace WeddingPlanner.Core.Domain.Entities;

public class PhoneNumber
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? AreaCode { get; set; }
    public string? Number { get; set; }

    public override string ToString() => $"+({AreaCode}) {Number}";
}