using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetProducts(int? categoryId)
    {
        List<ProductResponse> products = await _productService.GetProductsAsync(categoryId);

        return Ok(products);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        ProductResponse product = await _productService.GetProductByIdAsync(id);

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest createProductRequest)
    {
        ProductResponse createdProduct = await _productService.CreateProductAsync(createProductRequest);

        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest updateProductRequest)
    {
        ProductResponse updatedProduct = await _productService.UpdateProductAsync(id, updateProductRequest);

        return Ok(updatedProduct);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> AdjustStock(int id, AdjustStockRequest adjustStockRequest)
    {
        ProductResponse updatedProduct = await _productService.AdjustStockAsync(id, adjustStockRequest);

        return Ok(updatedProduct);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);

        return NoContent();
    }
}