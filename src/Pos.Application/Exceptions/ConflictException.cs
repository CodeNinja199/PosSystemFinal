namespace Pos.Application.Exceptions;

// Thrown by a service when the current data forbids the request. ErrorHandlingMiddleware turns it into 409.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}