using Microsoft.AspNetCore.Mvc;
using Pos.Api.Data;
using Pos.Api.Dtos;
using Pos.Domain.Entities;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts(int? categoryId)
    {
        List<ProductResponse> productsToReturn = new List<ProductResponse>();
        foreach (Product product in InMemoryData.Products)
        {
            bool isInRequestedCategory = categoryId == null || product.CategoryId == categoryId.Value;
            if (isInRequestedCategory)
            {
                ProductResponse productResponse = MapProductToResponse(product);
                productsToReturn.Add(productResponse);
            }
        }

        return Ok(productsToReturn);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id)
    {
        Product? productFromList = FindProductInList(id);

        if (productFromList == null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        ProductResponse productResponse = MapProductToResponse(productFromList);

        return Ok(productResponse);
    }

    private static Product? FindProductInList(int productId)
    {
        foreach (Product product in InMemoryData.Products)
        {
            if (product.Id == productId)
            {
                return product;
            }
        }

        return null;
    }

    private static ProductResponse MapProductToResponse(Product product)
    {
        ProductResponse productResponse = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId
        };

        return productResponse;
    }
}
