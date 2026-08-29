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
}