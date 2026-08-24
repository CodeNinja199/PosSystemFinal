using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        List<CategoryResponse> categories = await _categoryService.GetCategoriesAsync();

        return Ok(categories);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        CategoryResponse category = await _categoryService.GetCategoryByIdAsync(id);

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest createCategoryRequest)
    {
        CategoryResponse createdCategory = await _categoryService.CreateCategoryAsync(createCategoryRequest);

        return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest updateCategoryRequest)
    {
        CategoryResponse updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryRequest);

        return Ok(updatedCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);

        return NoContent();
    }
}