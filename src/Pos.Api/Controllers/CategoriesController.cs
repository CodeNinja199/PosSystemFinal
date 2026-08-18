using Microsoft.AspNetCore.Mvc;
using Pos.Api.Data;
using Pos.Api.Dtos;
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
