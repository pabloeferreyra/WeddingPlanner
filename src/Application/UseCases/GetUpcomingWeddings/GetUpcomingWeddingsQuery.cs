using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

public sealed class GetUpcomingWeddingsQuery
{
    [Required]
    public int Limit { get; }
}