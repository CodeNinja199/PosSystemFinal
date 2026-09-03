namespace Notification.Api.Exceptions;

// Thrown by NotificationsController when a notification does not exist. ErrorHandlingMiddleware turns it into 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}