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

    // The test database is migrated and seeded once and then kept, so every earlier run of these two
    // tests has already sold some of the seeded stock: a test that simply bought the first product
    // would eventually meet an empty shelf and fail with 409. Each test therefore takes a delivery in
    // as the admin first - the same endpoint an admin uses to receive one - and the returned product
    // carries the stock as it now stands. The Authorization header is left holding the admin's token,
    // so the caller sets its own afterwards.
    private async Task<ProductResponse> ReceiveStockAsTheAdminAsync(int howManyToReceive)
    {
        LoginRequest adminLoginRequest = new LoginRequest { Email = "admin@riversidemart.test", Password = "Admin#Test2026" };
        HttpResponseMessage adminLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", adminLoginRequest);
        LoginResponse? adminLoginBody = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(adminLoginBody);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLoginBody.Token);

        HttpResponseMessage productsResponse = await _client.GetAsync("/api/products");
        List<ProductResponse>? products = await productsResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(products);
        ProductResponse productToStock = products[0];

        AdjustStockRequest adjustStockRequest = new AdjustStockRequest { Change = howManyToReceive };
        HttpResponseMessage adjustStockResponse = await _client.PatchAsJsonAsync($"/api/products/{productToStock.Id}/stock", adjustStockRequest);
        Assert.Equal(HttpStatusCode.OK, adjustStockResponse.StatusCode);

        ProductResponse? stockedProduct = await adjustStockResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(stockedProduct);
        return stockedProduct;
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
        string buyerToken = loginBody.Token;

        ProductResponse productToBuy = await ReceiveStockAsTheAdminAsync(2);
        int stockBefore = productToBuy.StockQuantity;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);

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
        // Same reason as the test above: the shelf is stocked before anything is sold off it.
        ProductResponse productToSell = await ReceiveStockAsTheAdminAsync(1);

        LoginRequest cashierLoginRequest = new LoginRequest { Email = "cashier@riversidemart.test", Password = "Cashier#Test2026" };
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync("/api/auth/login", cashierLoginRequest);
        LoginResponse? loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

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