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
        int storeId = CurrentUserClaims.GetStoreId(User);

        List<CategoryResponse> categories = await _categoryService.GetCategoriesAsync(storeId);

        return Ok(categories);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        CategoryResponse category = await _categoryService.GetCategoryByIdAsync(id, storeId);

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest createCategoryRequest)
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        CategoryResponse createdCategory = await _categoryService.CreateCategoryAsync(createCategoryRequest, storeId);

        return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest updateCategoryRequest)
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        CategoryResponse updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryRequest, storeId);

        return Ok(updatedCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        await _categoryService.DeleteCategoryAsync(id, storeId);

        return NoContent();
    }
}