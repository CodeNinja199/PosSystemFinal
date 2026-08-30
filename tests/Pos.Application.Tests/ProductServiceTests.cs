using Moq;

using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;

namespace Pos.Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository;
    private readonly Mock<ICategoryRepository> _categoryRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepository = new Mock<IProductRepository>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _productService = new ProductService(_productRepository.Object, _categoryRepository.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_returns_the_product_when_it_exists_in_the_store()
    {
        Product productInStore = new Product { Id = 4, StoreId = 2, Name = "Chocolate bar", Price = 150, StockQuantity = 8, LowStockThreshold = 10, CategoryId = 5 };
        _productRepository
            .Setup(repository => repository.GetProductByIdAsync(4, 2))
            .ReturnsAsync(productInStore);

        ProductResponse productResponse = await _productService.GetProductByIdAsync(4, 2);

        string expectedName = "Chocolate bar";
        string actualName = productResponse.Name;
        Assert.Equal(expectedName, actualName);
        Assert.Equal(150, productResponse.Price);
    }

    [Fact]
    public async Task GetProductByIdAsync_throws_NotFoundException_when_the_repository_has_no_such_product()
    {
        Product? noProduct = null;
        _productRepository
            .Setup(repository => repository.GetProductByIdAsync(99, 2))
            .ReturnsAsync(noProduct);

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _productService.GetProductByIdAsync(99, 2);
        });

        string expectedMessage = "Product 99 was not found.";
        string actualMessage = exception.Message;
        Assert.Equal(expectedMessage, actualMessage);
    }
}