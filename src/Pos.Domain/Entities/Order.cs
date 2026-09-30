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

    // Copied at the time of the sale, like each line's name and price, so a later change of the GST rate never
    // changes an old receipt or an old day's takings (requirement 61). An order placed before GST existed has a
    // GST of zero and a subtotal equal to its total.
    public decimal Subtotal { get; set; }

    public decimal GstPercentage { get; set; }

    public decimal GstAmount { get; set; }

    // What the customer pays: the subtotal plus the GST.
    public decimal Total { get; set; }

    // Null for a card payment and for a customer's online order: there was no cash to count.
    public decimal? AmountTendered { get; set; }

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}