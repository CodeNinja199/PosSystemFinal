using Notification.Api.Exceptions;

namespace Notification.Api.Middleware;

// A copy of the POS API's error middleware with the two exceptions this service uses; copying twenty lines beats sharing a package.
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
        catch (ForbiddenException forbiddenException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden, forbiddenException.Message);
        }
        catch (NotFoundException notFoundException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, notFoundException.Message);
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