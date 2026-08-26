using Pos.Application.Exceptions;

namespace Pos.Api.Middleware;

// How the error middleware works here:
// 1. It wraps every request in one try/catch around _next, the way the Microsoft docs page "Write custom ASP.NET Core middleware" shows.
// 2. A named exception thrown by a service (Validation, Unauthorized, Forbidden, NotFound, Conflict) becomes its status code and a { message } body.
// 3. Anything else is logged and becomes 500 with "Something went wrong", so one failing request never crashes the application.
// Learned from: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // The JWT middleware and [Authorize] write a bare 401 or 403; give them the same { message } body as every other error.
            bool isUnauthorizedWithoutBody = context.Response.StatusCode == StatusCodes.Status401Unauthorized && context.Response.HasStarted == false;
            if (isUnauthorizedWithoutBody)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "A valid login token is required.");
            }

            bool isForbiddenWithoutBody = context.Response.StatusCode == StatusCodes.Status403Forbidden && context.Response.HasStarted == false;
            if (isForbiddenWithoutBody)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden, "Your role may not do this.");
            }
        }
        catch (ValidationException validationException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, validationException.Message);
        }
        catch (UnauthorizedException unauthorizedException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, unauthorizedException.Message);
        }
        catch (ForbiddenException forbiddenException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden, forbiddenException.Message);
        }
        catch (NotFoundException notFoundException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, notFoundException.Message);
        }
        catch (ConflictException conflictException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status409Conflict, conflictException.Message);
        }
        catch (Exception unexpectedException)
        {
            _logger.LogError(unexpectedException, "Unhandled exception while handling {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new { message = message });
    }
}