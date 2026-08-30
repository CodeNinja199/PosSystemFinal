using Moq;

using Pos.Application.Dtos;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Enums;

namespace Pos.Application.Tests;

public class ReportServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _userRepository = new Mock<IUserRepository>();
        _reportService = new ReportService(_orderRepository.Object, _userRepository.Object);
    }

    [Fact]
    public async Task GetTodaysSalesSummaryAsync_returns_the_total_and_count_the_repository_computed_for_the_store()
    {
        _orderRepository
            .Setup(repository => repository.GetSalesTotalBetweenAsync(2, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(570m);
        _orderRepository
            .Setup(repository => repository.GetOrderCountBetweenAsync(2, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(3);

        SalesSummaryResponse summary = await _reportService.GetTodaysSalesSummaryAsync(2);

        Assert.Equal(570m, summary.TotalSales);
        Assert.Equal(3, summary.OrderCount);
        Assert.Equal(DateTime.UtcNow.Date, summary.DayStartUtc);
    }

    [Fact]
    public async Task GetCustomersAsync_maps_each_customer_without_the_password_hash()
    {
        List<User> customers = new List<User>
        {
            new User { Id = 5, StoreId = 2, FullName = "Ana Perez", Email = "ana@example.com", PasswordHash = "$2a$11$hashed", Role = UserRole.Customer }
        };
        _userRepository
            .Setup(repository => repository.GetCustomersAsync(2))
            .ReturnsAsync(customers);

        List<UserResponse> customerResponses = await _reportService.GetCustomersAsync(2);

        Assert.Single(customerResponses);
        Assert.Equal("Ana Perez", customerResponses[0].FullName);
        Assert.Equal("Customer", customerResponses[0].Role);
    }
}