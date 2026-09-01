using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Notification.Api.Data;
using Notification.Api.Dtos;
using Notification.Api.Exceptions;

namespace Notification.Api.Controllers;

// One project, so the controller talks to the DbContext directly: two queries do not need a service and a repository between them.
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;

    public NotificationsController(NotificationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        int currentUserId = CurrentUserClaims.GetUserId(User);

        List<Entities.Notification> notificationsFromDatabase = await _context.Notifications
            .Where(notification => notification.RecipientUserId == currentUserId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync();

        List<NotificationResponse> notificationResponses = new List<NotificationResponse>();
        foreach (Entities.Notification notification in notificationsFromDatabase)
        {
            NotificationResponse notificationResponse = new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type,
                Message = notification.Message,
                CreatedAt = DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc),
                IsRead = notification.IsRead
            };
            notificationResponses.Add(notificationResponse);
        }

        return Ok(notificationResponses);
    }

    [Authorize]
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        int currentUserId = CurrentUserClaims.GetUserId(User);

        Entities.Notification? notificationFromDatabase = await _context.Notifications
            .FirstOrDefaultAsync(notification => notification.Id == id);
        if (notificationFromDatabase == null)
        {
            throw new NotFoundException($"Notification {id} was not found.");
        }

        if (notificationFromDatabase.RecipientUserId != currentUserId)
        {
            throw new ForbiddenException("This notification belongs to another user.");
        }

        notificationFromDatabase.IsRead = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}