using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class TimelineService(IRepository<Timeline> repository) : ITimelineService
{
    public async Task<IEnumerable<Timeline>> GetAllTimelinesAsync() => await repository.GetAllAsync();
    public async Task<Timeline> GetTimelineByIdAsync(Guid timelineId) => await repository.GetByIdAsync(timelineId);
    public async Task<Timeline> AddTimelineAsync(Timeline timeline) => await repository.AddAsync(timeline);
    public async Task<Timeline> UpdateTimelineAsync(Timeline timeline) => await repository.UpdateAsync(timeline);
    public async Task DeleteTimelineAsync(Guid timelineId) => await repository.DeleteAsync(timelineId);
    public async Task<bool> TimelineExistsAsync(Guid timelineId) => await repository.ExistsAsync(timelineId);
}
