namespace Pos.Application.Exceptions;

// Thrown by a service when a record does not exist. ErrorHandlingMiddleware turns it into 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}