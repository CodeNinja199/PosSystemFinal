namespace Pos.Application.Dtos;

public class SalesSummaryResponse
{
    public DateTime DayStartUtc { get; set; }

    public decimal TotalSales { get; set; }

    public int OrderCount { get; set; }
}