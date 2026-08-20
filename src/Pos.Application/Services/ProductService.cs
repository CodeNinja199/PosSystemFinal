using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

// Called by ProductsController. Holds every product rule; storage comes from the two repository interfaces.
public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<ProductResponse>> GetProductsAsync(int? categoryId)
    {
        List<Product> productsFromRepository = await _productRepository.GetProductsAsync(categoryId);

        List<ProductResponse> productResponses = new List<ProductResponse>();
        foreach (Product product in productsFromRepository)
        {
            ProductResponse productResponse = MapProductToResponse(product);
            productResponses.Add(productResponse);
        }

        return productResponses;
    }

    public async Task<ProductResponse> GetProductByIdAsync(int productId)
    {
        Product productFromRepository = await FindProductOrThrowAsync(productId);

        ProductResponse productResponse = MapProductToResponse(productFromRepository);

        return productResponse;
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest createProductRequest)
    {
        await EnsureCategoryExistsAsync(createProductRequest.CategoryId);

        Product newProduct = new Product
        {
            Name = createProductRequest.Name,
            Price = createProductRequest.Price,
            StockQuantity = createProductRequest.StockQuantity,
            LowStockThreshold = createProductRequest.LowStockThreshold,
            ImageUrl = createProductRequest.ImageUrl,
            CategoryId = createProductRequest.CategoryId
        };
        Product savedProduct = await _productRepository.AddProductAsync(newProduct);

        ProductResponse productResponse = MapProductToResponse(savedProduct);

        return productResponse;
    }

    public async Task<ProductResponse> UpdateProductAsync(int productId, UpdateProductRequest updateProductRequest)
    {
        Product productFromRepository = await FindProductOrThrowAsync(productId);

        await EnsureCategoryExistsAsync(updateProductRequest.CategoryId);

        productFromRepository.Name = updateProductRequest.Name;
        productFromRepository.Price = updateProductRequest.Price;
        productFromRepository.StockQuantity = updateProductRequest.StockQuantity;
        productFromRepository.LowStockThreshold = updateProductRequest.LowStockThreshold;
        productFromRepository.ImageUrl = updateProductRequest.ImageUrl;
        productFromRepository.CategoryId = updateProductRequest.CategoryId;
        await _productRepository.SaveProductAsync(productFromRepository);

        ProductResponse productResponse = MapProductToResponse(productFromRepository);

        return productResponse;
    }

    public async Task<ProductResponse> AdjustStockAsync(int productId, AdjustStockRequest adjustStockRequest)
    {
        if (adjustStockRequest.Change == 0)
        {
            throw new ValidationException("Change must not be zero.");
        }

        Product productFromRepository = await FindProductOrThrowAsync(productId);

        int newStockQuantity = productFromRepository.StockQuantity + adjustStockRequest.Change;
        if (newStockQuantity < 0)
        {
            throw new ConflictException($"Stock of {productFromRepository.Name} cannot go below zero.");
        }

        productFromRepository.StockQuantity = newStockQuantity;
        await _productRepository.SaveProductAsync(productFromRepository);

        ProductResponse productResponse = MapProductToResponse(productFromRepository);

        return productResponse;
    }

    public async Task DeleteProductAsync(int productId)
    {
        Product productFromRepository = await FindProductOrThrowAsync(productId);

        await _productRepository.DeleteProductAsync(productFromRepository);
    }

    private async Task<Product> FindProductOrThrowAsync(int productId)
    {
        Product? productFromRepository = await _productRepository.GetProductByIdAsync(productId);
        if (productFromRepository == null)
        {
            throw new NotFoundException($"Product {productId} was not found.");
        }

        return productFromRepository;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }
    }

    private static ProductResponse MapProductToResponse(Product product)
    {
        ProductResponse productResponse = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId
        };

        return productResponse;
    }
}