using System.ComponentModel.DataAnnotations;

using Pos.Domain.Enums;

namespace Pos.Application.Dtos;

public class UpdateOrderStatusRequest
{
    // Nullable so that a missing status fails [Required] instead of silently becoming Placed.
    [Required]
    public OrderStatus? Status { get; set; }
}