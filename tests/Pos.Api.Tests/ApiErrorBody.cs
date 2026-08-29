namespace Pos.Api.Tests;

// The shape of every error the API returns, read back in the tests.
public class ApiErrorBody
{
    public string Message { get; set; } = string.Empty;
}