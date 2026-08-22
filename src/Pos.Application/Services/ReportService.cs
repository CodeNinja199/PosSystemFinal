using Pos.Application.Dtos;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

// Called by ReportsController for the admin summary. Reads only; every number comes from a database query.
public class ReportService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public ReportService(IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    // "Today" is the current UTC day, because PlacedAt is stored in UTC.
    public async Task<SalesSummaryResponse> GetTodaysSalesSummaryAsync()
    {
        DateTime dayStartUtc = DateTime.UtcNow.Date;
        DateTime dayEndUtc = dayStartUtc.AddDays(1);

        decimal totalSales = await _orderRepository.GetSalesTotalBetweenAsync(dayStartUtc, dayEndUtc);
        int orderCount = await _orderRepository.GetOrderCountBetweenAsync(dayStartUtc, dayEndUtc);

        SalesSummaryResponse salesSummary = new SalesSummaryResponse
        {
            DayStartUtc = dayStartUtc,
            TotalSales = totalSales,
            OrderCount = orderCount
        };

        return salesSummary;
    }

    // Maps by hand, and the password hash is never copied.
    public async Task<List<UserResponse>> GetCustomersAsync()
    {
        List<User> customersFromRepository = await _userRepository.GetCustomersAsync();

        List<UserResponse> customerResponses = new List<UserResponse>();
        foreach (User customer in customersFromRepository)
        {
            UserResponse customerResponse = new UserResponse
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Role = customer.Role.ToString(),
                RegisteredAt = customer.RegisteredAt
            };
            customerResponses.Add(customerResponse);
        }

        return customerResponses;
    }
}