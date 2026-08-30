using System.Net;
using System.Net.Http.Json;

using Pos.Application.Dtos;

namespace Pos.Api.Tests;

public class AuthEndpointsTests : IClassFixture<PosApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(PosApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_then_login_returns_a_token_with_the_name_and_role()
    {
        string uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Test Customer", Email = uniqueEmail, Password = "secret123", StoreId = 1 };

        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        LoginRequest loginRequest = new LoginRequest { Email = uniqueEmail, Password = "secret123" };
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        LoginResponse? loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrEmpty(loginBody.Token));
        Assert.Equal("Test Customer", loginBody.FullName);
        Assert.Equal("Customer", loginBody.Role);
    }

    [Fact]
    public async Task Registering_the_same_email_twice_answers_409_with_a_message()
    {
        string uniqueEmail = $"twice-{Guid.NewGuid()}@example.com";
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Test Customer", Email = uniqueEmail, Password = "secret123", StoreId = 1 };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        HttpResponseMessage secondResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        ApiErrorBody? errorBody = await secondResponse.Content.ReadFromJsonAsync<ApiErrorBody>();
        Assert.NotNull(errorBody);
        Assert.Equal("Email is already registered.", errorBody.Message);
    }

    [Fact]
    public async Task Registering_with_a_blank_name_and_a_short_password_answers_400_naming_both_fields()
    {
        RegisterRequest registerRequest = new RegisterRequest { FullName = "", Email = "blank@example.com", Password = "short", StoreId = 1 };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ApiErrorBody? errorBody = await response.Content.ReadFromJsonAsync<ApiErrorBody>();
        Assert.NotNull(errorBody);
        Assert.Contains("FullName", errorBody.Message);
        Assert.Contains("Password", errorBody.Message);
    }
}