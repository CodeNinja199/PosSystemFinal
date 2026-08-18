using Microsoft.AspNetCore.Mvc;
using Pos.Api.Data;
using Pos.Domain.Entities;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts(int? categoryId)
    {
        List<Product> productsToReturn = new List<Product>();
        foreach (Product product in InMemoryData.Products)
        {
            if (categoryId == null)
            {
                productsToReturn.Add(product);
            }
            else if (product.CategoryId == categoryId.Value)
            {
                productsToReturn.Add(product);
            }
        }

        return Ok(productsToReturn);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id)
    {
        Product? productFromList = null;
        foreach (Product product in InMemoryData.Products)
        {
            if (product.Id == id)
            {
                productFromList = product;
            }
        }

        if (productFromList == null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        return Ok(productFromList);
    }
}
