using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Pos.Application.Dtos;
using Pos.Domain.Enums;

namespace Pos.Api.Tests;

public class CheckoutEndpointTests : IClassFixture<PosApiFactory>
{
    private readonly HttpClient _client;

    public CheckoutEndpointTests(PosApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Placing_an_order_reduces_stock_and_the_receipt_shows_the_copied_price()
    {
        string uniqueEmail = $"buyer-{Guid.NewGuid()}@example.com";
        RegisterRequest registerRequest = new RegisterRequest { FullName = "Test Buyer", Email = uniqueEmail, Password = "secret123", StoreId = 1 };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        LoginRequest loginRequest = new LoginRequest { Email = uniqueEmail, Password = "secret123" };
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        LoginResponse? loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

        HttpResponseMessage productsResponse = await _client.GetAsync("/api/products");
        List<ProductResponse>? products = await productsResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(products);
        ProductResponse productToBuy = products[0];
        int stockBefore = productToBuy.StockQuantity;

        PlaceOrderRequest placeOrderRequest = new PlaceOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = productToBuy.Id, Quantity = 2 } },
            PaymentMethod = PaymentMethod.Card
        };
        HttpResponseMessage orderResponse = await _client.PostAsJsonAsync("/api/orders", placeOrderRequest);

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        OrderResponse? placedOrder = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(placedOrder);
        Assert.Equal(productToBuy.Price * 2, placedOrder.Total);

        HttpResponseMessage receiptResponse = await _client.GetAsync($"/api/orders/{placedOrder.Id}");
        OrderResponse? receipt = await receiptResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(receipt);
        Assert.Equal(productToBuy.Name, receipt.Items[0].ProductName);
        Assert.Equal(productToBuy.Price, receipt.Items[0].UnitPrice);

        HttpResponseMessage productAfterResponse = await _client.GetAsync($"/api/products/{productToBuy.Id}");
        ProductResponse? productAfter = await productAfterResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(productAfter);
        Assert.Equal(stockBefore - 2, productAfter.StockQuantity);
    }
}