using Microsoft.AspNetCore.Mvc;
using Pos.Api.Data;
using Pos.Domain.Entities;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCategories()
    {
        List<Category> categoriesFromList = InMemoryData.Categories;

        return Ok(categoriesFromList);
    }

    [HttpGet("{id}")]
    public IActionResult GetCategoryById(int id)
    {
        Category? categoryFromList = null;
        foreach (Category category in InMemoryData.Categories)
        {
            if (category.Id == id)
            {
                categoryFromList = category;
            }
        }

        if (categoryFromList == null)
        {
            return NotFound(new { message = $"Category {id} was not found." });
        }

        return Ok(categoryFromList);
    }
}
