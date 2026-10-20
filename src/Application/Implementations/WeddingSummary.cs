namespace WeddingPlanner.Application.Implementations;

public sealed record WeddingSummary(
    Guid Id,
    string Name,
    DateTime Date,
    string Venue);