using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

// Called by CategoriesController. Holds every category rule; storage comes from ICategoryRepository.
public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        List<Category> categoriesFromRepository = await _categoryRepository.GetAllCategoriesAsync();

        List<CategoryResponse> categoryResponses = new List<CategoryResponse>();
        foreach (Category category in categoriesFromRepository)
        {
            CategoryResponse categoryResponse = MapCategoryToResponse(category);
            categoryResponses.Add(categoryResponse);
        }

        return categoryResponses;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(int categoryId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromRepository);

        return categoryResponse;
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest createCategoryRequest)
    {
        bool isNameTaken = await _categoryRepository.IsCategoryNameTakenAsync(createCategoryRequest.Name, null);
        if (isNameTaken)
        {
            throw new ConflictException($"A category named {createCategoryRequest.Name} already exists.");
        }

        Category newCategory = new Category
        {
            Name = createCategoryRequest.Name
        };
        Category savedCategory = await _categoryRepository.AddCategoryAsync(newCategory);

        CategoryResponse categoryResponse = MapCategoryToResponse(savedCategory);

        return categoryResponse;
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest updateCategoryRequest)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId);
        if (categoryFromRepository == null)
        {
            throw new NotFoundException($"Category {categoryId} was not found.");
        }

        bool isNameTaken = await _categoryRepository.IsCategoryNameTakenAsync(updateCategoryRequest.Name, categoryId);
        if (isNameTaken)
        {
            throw new ConflictException($"A category named {updateCategoryRequest.Name} already exists.");
        }

        categoryFromRepository.Name = updateCategoryRequest.Name;
        await _categoryRepository.SaveCategoryAsync(categoryFromRepository);

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromRepository);

        return categoryResponse;
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        Category? categoryFromRepository = await _categoryRepository.GetCategoryByIdAsync(categoryId);
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
