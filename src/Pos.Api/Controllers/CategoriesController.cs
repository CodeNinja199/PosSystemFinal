using Microsoft.AspNetCore.Mvc;
using Pos.Api.Data;
using Pos.Application.Dtos;
using Pos.Domain.Entities;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCategories()
    {
        List<CategoryResponse> categoriesToReturn = new List<CategoryResponse>();
        foreach (Category category in InMemoryData.Categories)
        {
            CategoryResponse categoryResponse = MapCategoryToResponse(category);
            categoriesToReturn.Add(categoryResponse);
        }

        return Ok(categoriesToReturn);
    }

    [HttpGet("{id}")]
    public IActionResult GetCategoryById(int id)
    {
        Category? categoryFromList = FindCategoryInList(id);

        if (categoryFromList == null)
        {
            return NotFound(new { message = $"Category {id} was not found." });
        }

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromList);

        return Ok(categoryResponse);
    }

    [HttpPost]
    public IActionResult CreateCategory(CreateCategoryRequest createCategoryRequest)
    {
        bool isNameAlreadyUsed = IsCategoryNameInList(createCategoryRequest.Name);
        if (isNameAlreadyUsed)
        {
            return Conflict(new { message = $"A category named {createCategoryRequest.Name} already exists." });
        }

        int nextCategoryId = 1;
        foreach (Category category in InMemoryData.Categories)
        {
            if (category.Id >= nextCategoryId)
            {
                nextCategoryId = category.Id + 1;
            }
        }

        Category newCategory = new Category
        {
            Id = nextCategoryId,
            Name = createCategoryRequest.Name
        };
        InMemoryData.Categories.Add(newCategory);

        CategoryResponse categoryResponse = MapCategoryToResponse(newCategory);

        return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, categoryResponse);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, UpdateCategoryRequest updateCategoryRequest)
    {
        Category? categoryFromList = FindCategoryInList(id);
        if (categoryFromList == null)
        {
            return NotFound(new { message = $"Category {id} was not found." });
        }

        categoryFromList.Name = updateCategoryRequest.Name;

        CategoryResponse categoryResponse = MapCategoryToResponse(categoryFromList);

        return Ok(categoryResponse);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        Category? categoryFromList = FindCategoryInList(id);
        if (categoryFromList == null)
        {
            return NotFound(new { message = $"Category {id} was not found." });
        }

        bool doesCategoryHaveProducts = DoesCategoryHaveProductsInList(id);
        if (doesCategoryHaveProducts)
        {
            return Conflict(new { message = $"Category {categoryFromList.Name} still has products." });
        }

        InMemoryData.Categories.Remove(categoryFromList);

        return NoContent();
    }

    private static bool DoesCategoryHaveProductsInList(int categoryId)
    {
        foreach (Product product in InMemoryData.Products)
        {
            if (product.CategoryId == categoryId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCategoryNameInList(string categoryName)
    {
        foreach (Category category in InMemoryData.Categories)
        {
            bool isSameName = string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase);
            if (isSameName)
            {
                return true;
            }
        }

        return false;
    }

    private static Category? FindCategoryInList(int categoryId)
    {
        foreach (Category category in InMemoryData.Categories)
        {
            if (category.Id == categoryId)
            {
                return category;
            }
        }

        return null;
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
