using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Application.UseCases.GetUpcomingWeddings;

public sealed class GetUpcomingWeddingsQuery(int limit)
{
    [Required]
    public int Limit { get; } = limit;
}