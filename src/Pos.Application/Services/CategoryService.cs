using Microsoft.Extensions.Caching.Memory;

using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

// How the category cache works here:
// 1. The category list of a store rarely changes and is read on every products page, so the first read of a store's list is kept
//    in memory under the key CategoriesCacheKeyPrefix + storeId for five minutes (an absolute expiry, so a stale list never outlives it).
// 2. Every change to a category of that store calls Remove on that key, so the next read goes to the database again.
// 3. Without the Remove, an admin who renamed a category could see the old name for up to five minutes: a cache that lies.
// Learned from: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory
public class CategoryService
{
    public const string CategoriesCacheKeyPrefix = "categories-of-store-";

    private static readonly TimeSpan CategoriesCacheLifetime = TimeSpan.FromMinutes(5);

    private readonly ICategoryRepository _categoryRepository;
    private readonly IMemoryCache _memoryCache;

    public CategoryService(ICategoryRepository categoryRepository, IMemoryCache memoryCache)
    {
        _categoryRepository = categoryRepository;
        _memoryCache = memoryCache;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync(int storeId)
    {
        string cacheKey = CategoriesCacheKeyPrefix + storeId;

        List<CategoryResponse>? cachedCategories;
        bool isInCache = _memoryCache.TryGetValue(cacheKey, out cachedCategories);
        if (isInCache && cachedCategories != null)
        {
            return cachedCategories;
        }

        List<Category> categoriesFromRepository = await _categoryRepository.GetAllCategoriesAsync(storeId);

        List<CategoryResponse> categoryResponses = new List<CategoryResponse>();
        foreach (Category category in categoriesFromRepository)
        {
            CategoryResponse categoryResponse = MapCategoryToResponse(category);
            categoryResponses.Add(categoryResponse);
        }

        _memoryCache.Set(cacheKey, categoryResponses, CategoriesCacheLifetime);

        return categoryResponses;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(int categoryId, int storeId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId, storeId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromRepository);

        return categoryResponse;
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest createCategoryRequest, int storeId)
    {
        bool isNameTaken = await _categoryRepository.IsCategoryNameTakenAsync(createCategoryRequest.Name, storeId, null);
        if (isNameTaken)
        {
            throw new ConflictException($"A category named {createCategoryRequest.Name} already exists.");
        }

        Category newCategory = new Category
        {
            StoreId = storeId,
            Name = createCategoryRequest.Name
        };
        Category savedCategory = await _categoryRepository.AddCategoryAsync(newCategory);

        RemoveCategoriesOfStoreFromCache(storeId);

        CategoryResponse categoryResponse = MapCategoryToResponse(savedCategory);

        return categoryResponse;
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest updateCategoryRequest, int storeId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId, storeId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }

        bool isNameTaken = await _categoryRepository.IsCategoryNameTakenAsync(updateCategoryRequest.Name, storeId, categoryId);
        if (isNameTaken)
        {
            throw new ConflictException($"A category named {updateCategoryRequest.Name} already exists.");
        }

        categoryFromRepository.Name = updateCategoryRequest.Name;
        await _categoryRepository.SaveCategoryAsync(categoryFromRepository);

        RemoveCategoriesOfStoreFromCache(storeId);

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromRepository);

        return categoryResponse;
    }

    public async Task DeleteCategoryAsync(int categoryId, int storeId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId, storeId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }

        bool doesCategoryHaveProducts = await _categoryRepository.DoesCategoryHaveProductsAsync(categoryId);
        if (doesCategoryHaveProducts)
        {
            throw new ConflictException($"Category {categoryFromRepository.Name} still has products.");
        }

        await _categoryRepository.DeleteCategoryAsync(categoryFromRepository);

        RemoveCategoriesOfStoreFromCache(storeId);
    }

    private void RemoveCategoriesOfStoreFromCache(int storeId)
    {
        string cacheKey = CategoriesCacheKeyPrefix + storeId;

        _memoryCache.Remove(cacheKey);
    }

    private static CategoryResponse MapCategoryToResponse(Category category)
    {
        CategoryResponse categoryResponse = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        };

        return categoryResponse;
    }
}