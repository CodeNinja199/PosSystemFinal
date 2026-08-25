using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by OrderRepository in Infrastructure. Used by OrderService and ReportService.
public interface IOrderRepository
{
    // One SaveChangesAsync writes the new order and every stock change already tracked by the same DbContext.
    Task SaveNewOrderAsync(Order order);

    Task<List<Order>> GetOrdersForUserAsync(int userId);

    Task<Order?> GetOrderByIdAsync(int orderId, int storeId);

    Task<List<Order>> GetAllOrdersAsync(int storeId);

    Task SaveOrderAsync(Order order);

    Task<decimal> GetSalesTotalBetweenAsync(int storeId, DateTime fromUtc, DateTime toUtc);

    Task<int> GetOrderCountBetweenAsync(int storeId, DateTime fromUtc, DateTime toUtc);
}