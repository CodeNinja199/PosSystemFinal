using System.ComponentModel.DataAnnotations;

namespace Pos.Api.Dtos;

public class AdjustStockRequest
{
    [Range(-1000000, 1000000)]
    public int Change { get; set; }
}
