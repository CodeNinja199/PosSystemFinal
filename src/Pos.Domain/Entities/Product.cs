namespace Pos.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int LowStockThreshold { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
}