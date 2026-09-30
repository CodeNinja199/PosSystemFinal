namespace Pos.Application.Dtos;

public class SalesSummaryResponse
{
    public DateTime DayStartUtc { get; set; }

    public decimal TotalSales { get; set; }

    // The GST inside TotalSales, shown beside it rather than hidden in one number (requirement 34).
    public decimal GstCollected { get; set; }

    public int OrderCount { get; set; }
}