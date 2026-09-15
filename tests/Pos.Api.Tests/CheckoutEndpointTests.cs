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

        OrderItemRequest orderItemRequest = new OrderItemRequest { ProductId = productToBuy.Id, Quantity = 2 };
        List<OrderItemRequest> orderItems = new List<OrderItemRequest> { orderItemRequest };
        PlaceOrderRequest placeOrderRequest = new PlaceOrderRequest
        {
            Items = orderItems,
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

    [Fact]
    public async Task A_cashiers_cash_sale_is_completed_at_once_and_the_receipt_shows_the_change()
    {
        LoginRequest cashierLoginRequest = new LoginRequest { Email = "cashier@riversidemart.test", Password = "Cashier#Test2026" };
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/auth/login", cashierLoginRequest);
        LoginResponse? loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

        HttpResponseMessage productsResponse = await _client.GetAsync("/api/products");
        List<ProductResponse>? products = await productsResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(products);
        ProductResponse productToSell = products[0];

        OrderItemRequest orderItemRequest = new OrderItemRequest { ProductId = productToSell.Id, Quantity = 1 };
        List<OrderItemRequest> orderItems = new List<OrderItemRequest> { orderItemRequest };
        PlaceOrderRequest walkInSaleRequest = new PlaceOrderRequest
        {
            Items = orderItems,
            PaymentMethod = PaymentMethod.Cash,
            AmountTendered = productToSell.Price + 50
        };
        HttpResponseMessage saleResponse = await _client.PostAsJsonAsync("/api/orders", walkInSaleRequest);

        Assert.Equal(HttpStatusCode.Created, saleResponse.StatusCode);
        OrderResponse? sale = await saleResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(sale);
        Assert.Equal("Completed", sale.Status);
        Assert.Equal<decimal?>(50m, sale.ChangeDue);

        HttpResponseMessage receiptResponse = await _client.GetAsync($"/api/orders/{sale.Id}");
        OrderResponse? receipt = await receiptResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(receipt);
        Assert.Equal<decimal?>(productToSell.Price + 50, receipt.AmountTendered);
        Assert.Equal<decimal?>(50m, receipt.ChangeDue);
    }
}