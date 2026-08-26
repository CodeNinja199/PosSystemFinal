using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by ProductRepository in Infrastructure. Used by ProductService and OrderService.
public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync(int storeId, int? categoryId);

    Task<Product?> GetProductByIdAsync(int productId, int storeId);

    Task<List<Product>> GetProductsByIdsAsync(List<int> productIds, int storeId);

    Task<bool> IsProductInAnyOrderAsync(int productId);

    Task<Product> AddProductAsync(Product product);

    Task SaveProductAsync(Product product);

    Task DeleteProductAsync(Product product);
}