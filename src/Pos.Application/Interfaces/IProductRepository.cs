using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by InMemoryProductRepository in Infrastructure, later by the EF Core ProductRepository.
public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync(int? categoryId);

    Task<Product?> GetProductByIdAsync(int productId);

    Task<Product> AddProductAsync(Product product);

    Task SaveProductAsync(Product product);

    Task DeleteProductAsync(Product product);
}
