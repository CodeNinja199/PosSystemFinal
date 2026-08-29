using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Enums;

namespace Pos.Application.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository;
    private readonly Mock<IProductRepository> _productRepository;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _productRepository = new Mock<IProductRepository>();
        _orderService = new OrderService(_orderRepository.Object, _productRepository.Object, NullLogger<OrderService>.Instance);
    }

    private static PlaceOrderRequest BuildRequest(int productId, int quantity)
    {
        PlaceOrderRequest placeOrderRequest = new PlaceOrderRequest
        {
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = productId, Quantity = quantity } },
            PaymentMethod = PaymentMethod.Cash
        };

        return placeOrderRequest;
    }

    [Fact]
    public async Task PlaceOrderAsync_throws_ValidationException_when_the_same_product_is_listed_twice()
    {
        PlaceOrderRequest placeOrderRequest = BuildRequest(4, 1);
        placeOrderRequest.Items.Add(new OrderItemRequest { ProductId = 4, Quantity = 2 });

        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _orderService.PlaceOrderAsync(placeOrderRequest, 5, 2);
        });

        Assert.Equal("Product 4 is listed more than once.", exception.Message);
        _orderRepository.Verify(repository => repository.SaveNewOrderAsync(It.IsAny<Order>()), Times.Never());
    }

    [Fact]
    public async Task PlaceOrderAsync_throws_NotFoundException_when_a_product_is_not_in_the_store()
    {
        _productRepository
            .Setup(repository => repository.GetProductsByIdsAsync(It.IsAny<List<int>>(), 2))
            .ReturnsAsync(new List<Product>());

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _orderService.PlaceOrderAsync(BuildRequest(999, 1), 5, 2);
        });

        Assert.Equal("Product 999 was not found.", exception.Message);
    }

    [Fact]
    public async Task PlaceOrderAsync_throws_ConflictException_and_saves_nothing_when_stock_is_too_low()
    {
        Product chocolateBar = new Product { Id = 4, StoreId = 2, Name = "Chocolate bar", Price = 150, StockQuantity = 8, LowStockThreshold = 10 };
        _productRepository
            .Setup(repository => repository.GetProductsByIdsAsync(It.IsAny<List<int>>(), 2))
            .ReturnsAsync(new List<Product> { chocolateBar });

        ConflictException exception = await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _orderService.PlaceOrderAsync(BuildRequest(4, 50), 5, 2);
        });

        Assert.Equal("Not enough stock of Chocolate bar: 8 left.", exception.Message);
        Assert.Equal(8, chocolateBar.StockQuantity);
        _orderRepository.Verify(repository => repository.SaveNewOrderAsync(It.IsAny<Order>()), Times.Never());
    }

    [Fact]
    public async Task PlaceOrderAsync_copies_the_price_reduces_stock_and_saves_once_when_everything_is_right()
    {
        Product chocolateBar = new Product { Id = 4, StoreId = 2, Name = "Chocolate bar", Price = 150, StockQuantity = 8, LowStockThreshold = 10 };
        _productRepository
            .Setup(repository => repository.GetProductsByIdsAsync(It.IsAny<List<int>>(), 2))
            .ReturnsAsync(new List<Product> { chocolateBar });

        OrderResponse orderResponse = await _orderService.PlaceOrderAsync(BuildRequest(4, 3), 5, 2);

        Assert.Equal(450, orderResponse.Total);
        Assert.Equal("Placed", orderResponse.Status);
        Assert.Equal("Chocolate bar", orderResponse.Items[0].ProductName);
        Assert.Equal(5, chocolateBar.StockQuantity);
        _orderRepository.Verify(repository => repository.SaveNewOrderAsync(It.IsAny<Order>()), Times.Once());
    }

    [Fact]
    public async Task GetOrderByIdAsync_throws_ForbiddenException_when_a_customer_opens_another_customers_order()
    {
        Order someoneElsesOrder = new Order { Id = 1, StoreId = 2, UserId = 5, Status = OrderStatus.Placed };
        _orderRepository
            .Setup(repository => repository.GetOrderByIdAsync(1, 2))
            .ReturnsAsync(someoneElsesOrder);

        await Assert.ThrowsAsync<ForbiddenException>(async () =>
        {
            await _orderService.GetOrderByIdAsync(1, 7, UserRole.Customer, 2);
        });
    }

    [Fact]
    public async Task GetOrderByIdAsync_returns_any_order_of_the_store_to_a_cashier()
    {
        Order someoneElsesOrder = new Order { Id = 1, StoreId = 2, UserId = 5, Status = OrderStatus.Placed };
        _orderRepository
            .Setup(repository => repository.GetOrderByIdAsync(1, 2))
            .ReturnsAsync(someoneElsesOrder);

        OrderResponse orderResponse = await _orderService.GetOrderByIdAsync(1, 7, UserRole.Cashier, 2);

        Assert.Equal(1, orderResponse.Id);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_throws_ConflictException_when_the_order_is_already_completed()
    {
        Order completedOrder = new Order { Id = 1, StoreId = 2, UserId = 5, Status = OrderStatus.Completed };
        _orderRepository
            .Setup(repository => repository.GetOrderByIdAsync(1, 2))
            .ReturnsAsync(completedOrder);
        UpdateOrderStatusRequest updateOrderStatusRequest = new UpdateOrderStatusRequest { Status = OrderStatus.Cancelled };

        ConflictException exception = await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _orderService.UpdateOrderStatusAsync(1, updateOrderStatusRequest, 2);
        });

        Assert.Equal("Order 1 is already Completed.", exception.Message);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_throws_ValidationException_when_the_status_is_not_an_end_state()
    {
        UpdateOrderStatusRequest updateOrderStatusRequest = new UpdateOrderStatusRequest { Status = OrderStatus.Placed };

        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _orderService.UpdateOrderStatusAsync(1, updateOrderStatusRequest, 2);
        });

        Assert.Equal("Status must be Completed or Cancelled.", exception.Message);
    }
}