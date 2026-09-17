namespace WeddingPlanner.Application.Weddings;

public sealed record WeddingSummary(
    string Name,
    DateTime Date,
    string Venue);