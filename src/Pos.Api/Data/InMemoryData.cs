using Pos.Domain.Entities;

namespace Pos.Api.Data;

// The lists are the data store until the database lecture.
// Read and changed by CategoriesController and ProductsController.
public static class InMemoryData
{
    public static List<Category> Categories = new List<Category>
    {
        new Category { Id = 1, Name = "Drinks" },
        new Category { Id = 2, Name = "Snacks" },
        new Category { Id = 3, Name = "Stationery" }
    };

    public static List<Product> Products = new List<Product>
    {
        new Product { Id = 1, Name = "Mineral water 500ml", Price = 60, StockQuantity = 120, LowStockThreshold = 20, CategoryId = 1 },
        new Product { Id = 2, Name = "Orange juice 1L", Price = 250, StockQuantity = 30, LowStockThreshold = 10, CategoryId = 1 },
        new Product { Id = 3, Name = "Salted chips 50g", Price = 80, StockQuantity = 200, LowStockThreshold = 40, CategoryId = 2 },
        new Product { Id = 4, Name = "Chocolate bar", Price = 150, StockQuantity = 8, LowStockThreshold = 10, CategoryId = 2 },
        new Product { Id = 5, Name = "Ballpoint pen", Price = 40, StockQuantity = 500, LowStockThreshold = 50, CategoryId = 3 },
        new Product { Id = 6, Name = "Notebook A5", Price = 220, StockQuantity = 45, LowStockThreshold = 15, CategoryId = 3 }
    };
}
