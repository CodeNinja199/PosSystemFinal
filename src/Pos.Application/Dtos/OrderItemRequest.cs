using System.ComponentModel.DataAnnotations;

namespace Pos.Application.Dtos;

public class OrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}