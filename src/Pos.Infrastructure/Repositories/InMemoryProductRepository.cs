using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Repositories;

// Registered as a singleton in Program.cs because the list is the data store until the database lecture.
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Mineral water 500ml", Price = 60, StockQuantity = 120, LowStockThreshold = 20, CategoryId = 1 },
        new Product { Id = 2, Name = "Orange juice 1L", Price = 250, StockQuantity = 30, LowStockThreshold = 10, CategoryId = 1 },
        new Product { Id = 3, Name = "Salted chips 50g", Price = 80, StockQuantity = 200, LowStockThreshold = 40, CategoryId = 2 },
        new Product { Id = 4, Name = "Chocolate bar", Price = 150, StockQuantity = 8, LowStockThreshold = 10, CategoryId = 2 },
        new Product { Id = 5, Name = "Ballpoint pen", Price = 40, StockQuantity = 500, LowStockThreshold = 50, CategoryId = 3 },
        new Product { Id = 6, Name = "Notebook A5", Price = 220, StockQuantity = 45, LowStockThreshold = 15, CategoryId = 3 }
    };

    public Task<List<Product>> GetProductsAsync(int? categoryId)
    {
        List<Product> matchingProducts = new List<Product>();
        foreach (Product product in _products)
        {
            bool isInRequestedCategory = categoryId == null || product.CategoryId == categoryId.Value;
            if (isInRequestedCategory)
            {
                matchingProducts.Add(product);
            }
        }

        return Task.FromResult(matchingProducts);
    }

    public Task<Product?> GetProductByIdAsync(int productId)
    {
        Product? productFromList = null;
        foreach (Product product in _products)
        {
            if (product.Id == productId)
            {
                productFromList = product;
            }
        }

        return Task.FromResult(productFromList);
    }

    public Task<Product> AddProductAsync(Product product)
    {
        int nextProductId = 1;
        foreach (Product existingProduct in _products)
        {
            if (existingProduct.Id >= nextProductId)
            {
                nextProductId = existingProduct.Id + 1;
            }
        }

        product.Id = nextProductId;
        _products.Add(product);

        return Task.FromResult(product);
    }

    public Task SaveProductAsync(Product product)
    {
        // The product object came from this list, so its changes are already in the list.
        return Task.CompletedTask;
    }

    public Task DeleteProductAsync(Product product)
    {
        _products.Remove(product);

        return Task.CompletedTask;
    }
}
