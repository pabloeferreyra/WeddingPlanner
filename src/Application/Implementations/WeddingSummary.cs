using WeddingPlanner.Core.Domain.Enums;

namespace WeddingPlanner.Application.Implementations;

public sealed record WeddingSummary(
    Guid Id,
    string Name,
    DateOnly Date,
    string Venue,
    int ExpectedGuestCount,
    WeddingStatus Status,
    bool IsWedding);