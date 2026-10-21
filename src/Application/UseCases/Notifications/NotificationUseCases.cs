using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.UseCases.Notifications;

public sealed class GetAllNotificationsHandler(INotificationService service)
{
    public Task<IEnumerable<Notification>> HandleAsync() => service.GetAllNotificationsAsync();
}

public sealed record GetNotificationByIdQuery(Guid NotificationId);

public sealed class GetNotificationByIdHandler(INotificationService service)
{
    public Task<Notification> HandleAsync(GetNotificationByIdQuery query) =>
        service.GetNotificationByIdAsync(query.NotificationId);
}

public sealed record AddNotificationCommand(Notification Notification);

public sealed class AddNotificationHandler(INotificationService service)
{
    public Task<Notification> HandleAsync(AddNotificationCommand command) =>
        service.AddNotificationAsync(command.Notification);
}

public sealed record UpdateNotificationCommand(Notification Notification);

public sealed class UpdateNotificationHandler(INotificationService service)
{
    public Task<Notification> HandleAsync(UpdateNotificationCommand command) =>
        service.UpdateNotificationAsync(command.Notification);
}

public sealed record DeleteNotificationCommand(Guid NotificationId);

public sealed class DeleteNotificationHandler(INotificationService service)
{
    public Task HandleAsync(DeleteNotificationCommand command) =>
        service.DeleteNotificationAsync(command.NotificationId);
}

public sealed record NotificationExistsQuery(Guid NotificationId);

public sealed class NotificationExistsHandler(INotificationService service)
{
    public Task<bool> HandleAsync(NotificationExistsQuery query) =>
        service.NotificationExistsAsync(query.NotificationId);
}
