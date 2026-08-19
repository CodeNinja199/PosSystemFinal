using System.ComponentModel.DataAnnotations;

namespace Pos.Application.Dtos;

public class AdjustStockRequest
{
    [Range(-1000000, 1000000)]
    public int Change { get; set; }
}
