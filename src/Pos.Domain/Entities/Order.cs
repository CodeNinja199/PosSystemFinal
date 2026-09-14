using Pos.Domain.Enums;

namespace Pos.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int UserId { get; set; }

    public DateTime PlacedAt { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Total { get; set; }

    // Null for a card payment and for a customer's online order: there was no cash to count.
    public decimal? AmountTendered { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}