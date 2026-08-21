namespace Pos.Infrastructure.SeedData;

// One entry of the "products" array in seed-data.json. The category is named, not numbered, because ids are assigned by the database.
public class SeedProduct
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int LowStockThreshold { get; set; }

    public string? ImageUrl { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}