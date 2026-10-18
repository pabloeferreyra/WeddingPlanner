using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Application.UseCases.CreateWedding;

public sealed class CreateWeddingCommand
{
    [Required]
    [StringLength(150)]
    public required string Title { get; init; }

    [Required]
    public required DateTime WeddingDate { get; init; }

    [Required]
    [StringLength(100)]
    public required string BrideName { get; init; }

    [Required]
    [StringLength(100)]
    public required string GroomName { get; init; }

    [Required]
    [StringLength(200)]
    public required string Street { get; init; }

    [Required]
    [StringLength(100)]
    public required string City { get; init; }

    [Required]
    [StringLength(100)]
    public required string State { get; init; }

    [Required]
    [StringLength(20)]
    public required string ZipCode { get; init; }

    [Range(0, int.MaxValue)]
    public int ExpectedGuestCount { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal InitialBudget { get; init; }
}