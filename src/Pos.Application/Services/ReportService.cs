using Pos.Application.Dtos;
using Pos.Application.Interfaces;

namespace Pos.Application.Services;

// Called by ReportsController for the admin summary. Reads only; every number comes from a database query.
public class ReportService
{
    private readonly IOrderRepository _orderRepository;

    public ReportService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
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
}