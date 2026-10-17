using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Core.Domain.Entities;

namespace WeddingPlanner.Application.Implementations;

public sealed class NotificationService(IRepository<Notification> repository) : INotificationService
{
    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync() => await repository.GetAllAsync();
    public async Task<Notification> GetNotificationByIdAsync(Guid notificationId) => await repository.GetByIdAsync(notificationId);
    public async Task<Notification> AddNotificationAsync(Notification notification) => await repository.AddAsync(notification);
    public async Task<Notification> UpdateNotificationAsync(Notification notification) => await repository.UpdateAsync(notification);
    public async Task DeleteNotificationAsync(Guid notificationId) => await repository.DeleteAsync(notificationId);
    public async Task<bool> NotificationExistsAsync(Guid notificationId) => await repository.ExistsAsync(notificationId);
}
