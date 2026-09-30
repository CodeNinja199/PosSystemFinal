using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Pos.Application.Dtos;

namespace Pos.Api.Tests;

// Requirement 58: one GST rate for the whole system, readable by anyone logged in so the checkout screen can show it.
public class TaxEndpointTests : IClassFixture<PosApiFactory>
{
    private readonly HttpClient _client;

    public TaxEndpointTests(PosApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCustomerAsync()
    {
        string uniqueEmail = $"gst-reader-{Guid.NewGuid()}@example.com";
        RegisterRequest registerRequest = new RegisterRequest { FullName = "GST Reader", Email = uniqueEmail, Password = "secret123", StoreId = 1 };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        LoginRequest loginRequest = new LoginRequest { Email = uniqueEmail, Password = "secret123" };
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        LoginResponse? loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        if (loginBody == null)
        {
            throw new InvalidOperationException("Login returned no body.");
        }

        return loginBody.Token;
    }

    [Fact]
    public async Task A_customer_can_read_the_gst_rate()
    {
        string customerToken = await RegisterAndLoginCustomerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        HttpResponseMessage response = await _client.GetAsync("/api/tax");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(18m, body.GetProperty("gstPercentage").GetDecimal());
    }

    [Fact]
    public async Task Reading_the_gst_rate_without_a_token_answers_401()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/tax");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}