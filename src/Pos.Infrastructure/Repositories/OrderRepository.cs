using Microsoft.EntityFrameworkCore;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Enums;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.Repositories;

// Registered as scoped in Program.cs because it holds the scoped PosDbContext.
public class OrderRepository : IOrderRepository
{
    private readonly PosDbContext _context;

    public OrderRepository(PosDbContext context)
    {
        _context = context;
    }

    // The products changed by OrderService are tracked by this same context, so this one save writes them too.
    public async Task SaveNewOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetOrdersForUserAsync(int userId)
    {
        List<Order> ordersFromDatabase = await _context.Orders
            .Include(order => order.Items)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.PlacedAt)
            .ToListAsync();

        return ordersFromDatabase;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId, int storeId)
    {
        Order? orderFromDatabase = await _context.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId && order.StoreId == storeId);

        return orderFromDatabase;
    }

    public async Task<List<Order>> GetAllOrdersAsync(int storeId)
    {
        List<Order> ordersFromDatabase = await _context.Orders
            .Include(order => order.Items)
            .Where(order => order.StoreId == storeId)
            .OrderByDescending(order => order.PlacedAt)
            .ToListAsync();

        return ordersFromDatabase;
    }

    public async Task SaveOrderAsync(Order order)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetSalesTotalBetweenAsync(int storeId, DateTime fromUtc, DateTime toUtc)
    {
        decimal salesTotal = await _context.Orders
            .Where(order => order.StoreId == storeId && order.PlacedAt >= fromUtc && order.PlacedAt < toUtc && order.Status != OrderStatus.Cancelled)
            .SumAsync(order => order.Total);

        return salesTotal;
    }

    public async Task<int> GetOrderCountBetweenAsync(int storeId, DateTime fromUtc, DateTime toUtc)
    {
        int orderCount = await _context.Orders
            .Where(order => order.StoreId == storeId && order.PlacedAt >= fromUtc && order.PlacedAt < toUtc && order.Status != OrderStatus.Cancelled)
            .CountAsync();

        return orderCount;
    }
}