using Microsoft.EntityFrameworkCore;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.Repositories;

// Registered as scoped in Program.cs because it holds the scoped PosDbContext.
public class ProductRepository : IProductRepository
{
    private readonly PosDbContext _context;

    public ProductRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync(int? categoryId)
    {
        if (categoryId == null)
        {
            List<Product> allProducts = await _context.Products
                .OrderBy(product => product.Name)
                .ToListAsync();

            return allProducts;
        }

        List<Product> productsInCategory = await _context.Products
            .Where(product => product.CategoryId == categoryId.Value)
            .OrderBy(product => product.Name)
            .ToListAsync();

        return productsInCategory;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        Product? productFromDatabase = await _context.Products
            .FirstOrDefaultAsync(product => product.Id == productId);

        return productFromDatabase;
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<int> productIds)
    {
        List<Product> productsFromDatabase = await _context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync();

        return productsFromDatabase;
    }

    public async Task<bool> IsProductInAnyOrderAsync(int productId)
    {
        bool isProductInAnyOrder = await _context.OrderItems
            .AnyAsync(orderItem => orderItem.ProductId == productId);

        return isProductInAnyOrder;
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task SaveProductAsync(Product product)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}