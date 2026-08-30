using Microsoft.Extensions.Caching.Memory;

using Moq;

using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Application.Services;
using Pos.Domain.Entities;

namespace Pos.Application.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepository = new Mock<ICategoryRepository>();
        MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        _categoryService = new CategoryService(_categoryRepository.Object, memoryCache);
    }

    [Fact]
    public async Task CreateCategoryAsync_throws_ConflictException_when_the_name_is_already_used_in_the_store()
    {
        _categoryRepository
            .Setup(repository => repository.IsCategoryNameTakenAsync("Drinks", 1, null))
            .ReturnsAsync(true);
        CreateCategoryRequest createCategoryRequest = new CreateCategoryRequest { Name = "Drinks" };

        ConflictException exception = await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _categoryService.CreateCategoryAsync(createCategoryRequest, 1);
        });

        string expectedMessage = "A category named Drinks already exists.";
        Assert.Equal(expectedMessage, exception.Message);
        _categoryRepository.Verify(repository => repository.AddCategoryAsync(It.IsAny<Category>()), Times.Never());
    }

    [Fact]
    public async Task DeleteCategoryAsync_throws_ConflictException_when_the_category_still_has_products()
    {
        Category drinks = new Category { Id = 1, StoreId = 1, Name = "Drinks" };
        _categoryRepository
            .Setup(repository => repository.GetCategoryByIdAsync(1, 1))
            .ReturnsAsync(drinks);
        _categoryRepository
            .Setup(repository => repository.DoesCategoryHaveProductsAsync(1))
            .ReturnsAsync(true);

        ConflictException exception = await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _categoryService.DeleteCategoryAsync(1, 1);
        });

        string expectedMessage = "Category Drinks still has products.";
        Assert.Equal(expectedMessage, exception.Message);
        _categoryRepository.Verify(repository => repository.DeleteCategoryAsync(drinks), Times.Never());
    }

    [Fact]
    public async Task GetCategoriesAsync_reads_the_repository_once_and_serves_the_second_call_from_the_cache()
    {
        List<Category> categoriesInStore = new List<Category>
        {
            new Category { Id = 1, StoreId = 1, Name = "Drinks" }
        };
        _categoryRepository
            .Setup(repository => repository.GetAllCategoriesAsync(1))
            .ReturnsAsync(categoriesInStore);

        List<CategoryResponse> firstCall = await _categoryService.GetCategoriesAsync(1);
        List<CategoryResponse> secondCall = await _categoryService.GetCategoriesAsync(1);

        int expectedCount = 1;
        Assert.Equal(expectedCount, firstCall.Count);
        Assert.Equal(expectedCount, secondCall.Count);
        _categoryRepository.Verify(repository => repository.GetAllCategoriesAsync(1), Times.Once());
    }

    [Fact]
    public async Task CreateCategoryAsync_clears_the_cache_so_the_next_read_goes_to_the_repository()
    {
        List<Category> categoriesInStore = new List<Category>();
        _categoryRepository
            .Setup(repository => repository.GetAllCategoriesAsync(1))
            .ReturnsAsync(categoriesInStore);
        _categoryRepository
            .Setup(repository => repository.IsCategoryNameTakenAsync("Stationery", 1, null))
            .ReturnsAsync(false);
        Category savedCategory = new Category { Id = 9, StoreId = 1, Name = "Stationery" };
        _categoryRepository
            .Setup(repository => repository.AddCategoryAsync(It.IsAny<Category>()))
            .ReturnsAsync(savedCategory);
        CreateCategoryRequest createCategoryRequest = new CreateCategoryRequest { Name = "Stationery" };

        await _categoryService.GetCategoriesAsync(1);
        await _categoryService.CreateCategoryAsync(createCategoryRequest, 1);
        await _categoryService.GetCategoriesAsync(1);

        _categoryRepository.Verify(repository => repository.GetAllCategoriesAsync(1), Times.Exactly(2));
    }
}