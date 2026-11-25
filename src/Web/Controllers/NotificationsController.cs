using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Application.UseCases.Notifications;
using WeddingPlanner.Web.Models;

namespace WeddingPlanner.Web.Controllers;

[Authorize]
[Route("Notifications")]
public sealed class NotificationsController(
    GetAllNotificationsHandler getAllNotifications,
    GetNotificationByIdHandler getNotificationById,
    UpdateNotificationHandler updateNotification,
    DeleteNotificationHandler deleteNotification)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Forbid();
        }

        var notifications = (await getAllNotifications.HandleAsync())
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToList();
        return View(new NotificationListViewModel { Notifications = notifications });
    }

    [HttpPost("{id:guid}/Read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var notification = await getNotificationById.HandleAsync(new GetNotificationByIdQuery(id));
        if (notification is null || notification.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return NotFound();
        }

        notification.IsRead = true;
        notification.ReadAt ??= DateTime.Now;
        await updateNotification.HandleAsync(new UpdateNotificationCommand(notification));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var notification = await getNotificationById.HandleAsync(new GetNotificationByIdQuery(id));
        if (notification is null || notification.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return NotFound();
        }

        await deleteNotification.HandleAsync(new DeleteNotificationCommand(id));
        return RedirectToAction(nameof(Index));
    }
}
