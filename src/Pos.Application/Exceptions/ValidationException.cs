namespace Pos.Application.Exceptions;

// Thrown by a service for a check DataAnnotations cannot express. ErrorHandlingMiddleware turns it into 400.
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}