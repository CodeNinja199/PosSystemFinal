using Pos.Domain.Enums;

namespace Pos.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime PlacedAt { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Total { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}
