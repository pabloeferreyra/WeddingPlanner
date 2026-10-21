using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Timelines;

public sealed class GetAllTimelinesHandler(ITimelineService service)
{
    public Task<IEnumerable<Timeline>> HandleAsync() => service.GetAllTimelinesAsync();
}

public sealed record GetTimelineByIdQuery(Guid TimelineId);

public sealed class GetTimelineByIdHandler(ITimelineService service)
{
    public Task<Timeline> HandleAsync(GetTimelineByIdQuery query) => service.GetTimelineByIdAsync(query.TimelineId);
}

public sealed record AddTimelineCommand(Timeline Timeline);

public sealed class AddTimelineHandler(ITimelineService service)
{
    public Task<Timeline> HandleAsync(AddTimelineCommand command) => service.AddTimelineAsync(command.Timeline);
}

public sealed record UpdateTimelineCommand(Timeline Timeline);

public sealed class UpdateTimelineHandler(ITimelineService service)
{
    public Task<Timeline> HandleAsync(UpdateTimelineCommand command) => service.UpdateTimelineAsync(command.Timeline);
}

public sealed record DeleteTimelineCommand(Guid TimelineId);

public sealed class DeleteTimelineHandler(ITimelineService service)
{
    public Task HandleAsync(DeleteTimelineCommand command) => service.DeleteTimelineAsync(command.TimelineId);
}

public sealed record TimelineExistsQuery(Guid TimelineId);

public sealed class TimelineExistsHandler(ITimelineService service)
{
    public Task<bool> HandleAsync(TimelineExistsQuery query) => service.TimelineExistsAsync(query.TimelineId);
}
