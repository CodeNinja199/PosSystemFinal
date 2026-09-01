namespace Notification.Api.Exceptions;

// Thrown when a notification belongs to another user. ErrorHandlingMiddleware turns it into 403.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}