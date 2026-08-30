using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Pos.Application.Dtos;

namespace Pos.Api.Tests;

public class ProtectedEndpointsTests : IClassFixture<PosApiFactory>
{
    private readonly HttpClient _client;

    public ProtectedEndpointsTests(PosApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Registers a fresh customer in store 1 and returns their token.
    private async Task<string> RegisterAndLoginCustomerAsync()
    {
        string uniqueEmail = $"customer-{Guid.NewGuid()}@example.com";
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Test Customer", Email = uniqueEmail, Password = "secret123", StoreId = 1 };
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
    public async Task Products_answer_401_with_a_message_when_no_token_is_sent()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        ApiErrorBody? errorBody = await response.Content.ReadFromJsonAsync<ApiErrorBody>();
        Assert.NotNull(errorBody);
        Assert.Equal("A valid login token is required.", errorBody.Message);
    }

    [Fact]
    public async Task Creating_a_product_as_a_customer_answers_403_with_a_message()
    {
        string customerToken = await RegisterAndLoginCustomerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        CreateProductRequest createProductRequest = new CreateProductRequest { Name = "Not allowed", Price = 10, StockQuantity = 1, LowStockThreshold = 0, CategoryId = 1 };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", createProductRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        ApiErrorBody? errorBody = await response.Content.ReadFromJsonAsync<ApiErrorBody>();
        Assert.NotNull(errorBody);
        Assert.Equal("Your role may not do this.", errorBody.Message);
    }
}