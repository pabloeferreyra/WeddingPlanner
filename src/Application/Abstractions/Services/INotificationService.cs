using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Abstractions.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();
    Task<Notification> GetNotificationByIdAsync(Guid notificationId);
    Task<Notification> AddNotificationAsync(Notification notification);
    Task<Notification> UpdateNotificationAsync(Notification notification);
    Task DeleteNotificationAsync(Guid notificationId);
    Task<bool> NotificationExistsAsync(Guid notificationId);
}
