using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface ITimelineService
{
    Task<IEnumerable<Timeline>> GetAllTimelinesAsync();
    Task<Timeline> GetTimelineByIdAsync(Guid timelineId);
    Task<Timeline> AddTimelineAsync(Timeline timeline);
    Task<Timeline> UpdateTimelineAsync(Timeline timeline);
    Task DeleteTimelineAsync(Guid timelineId);
    Task<bool> TimelineExistsAsync(Guid timelineId);
}
