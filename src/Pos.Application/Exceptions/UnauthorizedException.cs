namespace Pos.Application.Exceptions;

// Thrown by AuthService when the email or password is wrong. ErrorHandlingMiddleware turns it into 401.
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}