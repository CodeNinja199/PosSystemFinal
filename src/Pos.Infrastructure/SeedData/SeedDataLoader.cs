using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.SeedData;

// How the seed loader works here:
// 1. Program.cs asks for this class once at startup and calls LoadAsync before the API starts listening.
// 2. If the Categories table already has rows, nothing happens: the seed only fills an empty database.
// 3. Otherwise it reads seed-data.json, deserializes it (JSON text -> C# objects), and saves categories first so they get ids, then products.
// Serialize means C# object -> JSON text; deserialize means JSON text -> C# object. PropertyNameCaseInsensitive is set because
// the file uses camelCase names ("stockQuantity") while the C# classes use PascalCase ("StockQuantity"), and matching is case-sensitive by default.
// Learned from: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization
// and https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-casing
public class SeedDataLoader
{
    private readonly PosDbContext _context;

    public SeedDataLoader(PosDbContext context)
    {
        _context = context;
    }

    public async Task LoadAsync()
    {
        bool hasCategoriesAlready = await _context.Categories.AnyAsync();
        if (hasCategoriesAlready)
        {
            return;
        }

        string seedFilePath = Path.Combine(AppContext.BaseDirectory, "SeedData", "seed-data.json");
        string seedJson = await File.ReadAllTextAsync(seedFilePath);

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        SeedDataFile? seedDataFile = JsonSerializer.Deserialize<SeedDataFile>(seedJson, jsonOptions);
        if (seedDataFile == null)
        {
            throw new InvalidOperationException("seed-data.json could not be read.");
        }

        Dictionary<string, Category> categoriesByName = new Dictionary<string, Category>();
        foreach (SeedCategory seedCategory in seedDataFile.Categories)
        {
            Category category = new Category
            {
                Name = seedCategory.Name
            };
            _context.Categories.Add(category);
            categoriesByName.Add(seedCategory.Name, category);
        }
        await _context.SaveChangesAsync();

        foreach (SeedProduct seedProduct in seedDataFile.Products)
        {
            Category categoryForProduct = categoriesByName[seedProduct.CategoryName];
            Product product = new Product
            {
                Name = seedProduct.Name,
                Price = seedProduct.Price,
                StockQuantity = seedProduct.StockQuantity,
                LowStockThreshold = seedProduct.LowStockThreshold,
                ImageUrl = seedProduct.ImageUrl,
                CategoryId = categoryForProduct.Id
            };
            _context.Products.Add(product);
        }
        await _context.SaveChangesAsync();
    }
}