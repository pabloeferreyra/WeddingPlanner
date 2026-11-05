namespace WeddingPlanner.Application.Implementations;

public sealed record WeddingSummary(
    Guid Id,
    string Name,
    DateOnly Date,
    string Venue);