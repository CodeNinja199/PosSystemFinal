namespace Pos.Application.Exceptions;

// Thrown by a service when a logged-in user may not touch this record. ErrorHandlingMiddleware turns it into 403.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}