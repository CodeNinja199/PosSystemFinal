using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.Repositories;

// Registered as scoped in Program.cs because it holds the scoped PosDbContext.
public class CategoryRepository : ICategoryRepository
{
    private readonly PosDbContext _context;

    public CategoryRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        List<Category> allCategories = await _context.Categories
            .OrderBy(category => category.Name)
            .ToListAsync();

        return allCategories;
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        Category? categoryFromDatabase = await _context.Categories
            .FirstOrDefaultAsync(category => category.Id == categoryId);

        return categoryFromDatabase;
    }

    // SQL Server compares strings without case by default, so "drinks" matches "Drinks" here as it did in the list.
    public async Task<bool> IsCategoryNameTakenAsync(string categoryName, int? categoryIdToIgnore)
    {
        if (categoryIdToIgnore == null)
        {
            bool isNameTaken = await _context.Categories
                .AnyAsync(category => category.Name == categoryName);

            return isNameTaken;
        }

        bool isNameTakenByAnotherCategory = await _context.Categories
            .AnyAsync(category => category.Name == categoryName && category.Id != categoryIdToIgnore.Value);

        return isNameTakenByAnotherCategory;
    }

    public async Task<bool> DoesCategoryHaveProductsAsync(int categoryId)
    {
        bool doesCategoryHaveProducts = await _context.Products
            .AnyAsync(product => product.CategoryId == categoryId);

        return doesCategoryHaveProducts;
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task SaveCategoryAsync(Category category)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}