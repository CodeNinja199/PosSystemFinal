using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by CategoryRepository in Infrastructure. Used by CategoryService and ProductService.
public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync(int storeId);

    Task<Category?> GetCategoryByIdAsync(int categoryId, int storeId);

    Task<bool> IsCategoryNameTakenAsync(string categoryName, int storeId, int? categoryIdToIgnore);

    Task<bool> DoesCategoryHaveProductsAsync(int categoryId);

    Task<Category> AddCategoryAsync(Category category);

    Task SaveCategoryAsync(Category category);

    Task DeleteCategoryAsync(Category category);
}