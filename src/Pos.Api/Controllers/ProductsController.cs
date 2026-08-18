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

    [HttpPost]
    public IActionResult CreateProduct(CreateProductRequest createProductRequest)
    {
        bool doesCategoryExist = DoesCategoryExistInList(createProductRequest.CategoryId);
        if (doesCategoryExist == false)
        {
            return NotFound(new { message = $"Category {createProductRequest.CategoryId} was not found." });
        }

        int nextProductId = 1;
        foreach (Product product in InMemoryData.Products)
        {
            if (product.Id >= nextProductId)
            {
                nextProductId = product.Id + 1;
            }
        }

        Product newProduct = new Product
        {
            Id = nextProductId,
            Name = createProductRequest.Name,
            Price = createProductRequest.Price,
            StockQuantity = createProductRequest.StockQuantity,
            LowStockThreshold = createProductRequest.LowStockThreshold,
            ImageUrl = createProductRequest.ImageUrl,
            CategoryId = createProductRequest.CategoryId
        };
        InMemoryData.Products.Add(newProduct);

        ProductResponse productResponse = MapProductToResponse(newProduct);

        return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, productResponse);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, UpdateProductRequest updateProductRequest)
    {
        Product? productFromList = FindProductInList(id);
        if (productFromList == null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        bool doesCategoryExist = DoesCategoryExistInList(updateProductRequest.CategoryId);
        if (doesCategoryExist == false)
        {
            return NotFound(new { message = $"Category {updateProductRequest.CategoryId} was not found." });
        }

        productFromList.Name = updateProductRequest.Name;
        productFromList.Price = updateProductRequest.Price;
        productFromList.StockQuantity = updateProductRequest.StockQuantity;
        productFromList.LowStockThreshold = updateProductRequest.LowStockThreshold;
        productFromList.ImageUrl = updateProductRequest.ImageUrl;
        productFromList.CategoryId = updateProductRequest.CategoryId;

        ProductResponse productResponse = MapProductToResponse(productFromList);

        return Ok(productResponse);
    }

    [HttpPatch("{id}/stock")]
    public IActionResult AdjustStock(int id, AdjustStockRequest adjustStockRequest)
    {
        if (adjustStockRequest.Change == 0)
        {
            return BadRequest(new { message = "Change must not be zero." });
        }

        Product? productFromList = FindProductInList(id);
        if (productFromList == null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        int newStockQuantity = productFromList.StockQuantity + adjustStockRequest.Change;
        if (newStockQuantity < 0)
        {
            return Conflict(new { message = $"Stock of {productFromList.Name} cannot go below zero." });
        }

        productFromList.StockQuantity = newStockQuantity;

        ProductResponse productResponse = MapProductToResponse(productFromList);

        return Ok(productResponse);
    }

    private static bool DoesCategoryExistInList(int categoryId)
    {
        foreach (Category category in InMemoryData.Categories)
        {
            if (category.Id == categoryId)
            {
                return true;
            }
        }

        return false;
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
