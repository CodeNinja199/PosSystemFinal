using System.ComponentModel.DataAnnotations;

using Pos.Domain.Enums;

namespace Pos.Application.Dtos;

public class PlaceOrderRequest
{
    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();

    // Nullable so that a missing payment method fails [Required] instead of silently becoming Cash.
    [Required]
    public PaymentMethod? PaymentMethod { get; set; }

    // Nullable: a card payment and a customer's online order carry no cash. DataAnnotations skip a null, so the range only checks a value.
    [Range(0.01, 100000000)]
    public decimal? AmountTendered { get; set; }
}