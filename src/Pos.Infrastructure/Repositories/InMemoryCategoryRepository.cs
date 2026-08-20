using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Repositories;

// Registered as a singleton in Program.cs because the list is the data store until the database lecture.
// It asks IProductRepository whether a category still has products, because the products live in the other list.
public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly IProductRepository _productRepository;

    private readonly List<Category> _categories = new List<Category>
    {
        new Category { Id = 1, Name = "Drinks" },
        new Category { Id = 2, Name = "Snacks" },
        new Category { Id = 3, Name = "Stationery" }
    };

    public InMemoryCategoryRepository(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<List<Category>> GetAllCategoriesAsync()
    {
        List<Category> allCategories = new List<Category>(_categories);

        return Task.FromResult(allCategories);
    }

    public Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        Category? categoryFromList = null;
        foreach (Category category in _categories)
        {
            if (category.Id == categoryId)
            {
                categoryFromList = category;
            }
        }

        return Task.FromResult(categoryFromList);
    }

    public Task<bool> IsCategoryNameTakenAsync(string categoryName, int? categoryIdToIgnore)
    {
        foreach (Category category in _categories)
        {
            bool isSameName = string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase);
            bool isIgnored = categoryIdToIgnore != null && category.Id == categoryIdToIgnore.Value;
            if (isSameName && isIgnored == false)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public async Task<bool> DoesCategoryHaveProductsAsync(int categoryId)
    {
        List<Product> productsInCategory = await _productRepository.GetProductsAsync(categoryId);

        bool doesCategoryHaveProducts = productsInCategory.Count > 0;

        return doesCategoryHaveProducts;
    }

    public Task<Category> AddCategoryAsync(Category category)
    {
        int nextCategoryId = 1;
        foreach (Category existingCategory in _categories)
        {
            if (existingCategory.Id >= nextCategoryId)
            {
                nextCategoryId = existingCategory.Id + 1;
            }
        }

        category.Id = nextCategoryId;
        _categories.Add(category);

        return Task.FromResult(category);
    }

    public Task SaveCategoryAsync(Category category)
    {
        // The category object came from this list, so its changes are already in the list.
        return Task.CompletedTask;
    }

    public Task DeleteCategoryAsync(Category category)
    {
        _categories.Remove(category);

        return Task.CompletedTask;
    }
}