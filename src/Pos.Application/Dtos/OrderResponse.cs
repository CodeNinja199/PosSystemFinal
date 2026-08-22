namespace Pos.Application.Dtos;

public class OrderResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime PlacedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
}