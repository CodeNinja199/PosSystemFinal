using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Enums;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.SeedData;

// How the seed loader works here:
// 1. Program.cs asks for this class once at startup and calls LoadAsync before the API starts listening.
// 2. If the Categories table already has rows, nothing happens: the seed only fills an empty database.
// 3. Otherwise it reads seed-data.json, deserializes it (JSON text -> C# objects), and saves categories first so they get ids, then products.
// 4. If the Users table is empty it also creates the staff from the "users" array, hashing the passwords found in the Seed section of configuration.
// Serialize means C# object -> JSON text; deserialize means JSON text -> C# object. PropertyNameCaseInsensitive is set because
// the file uses camelCase names ("stockQuantity") while the C# classes use PascalCase ("StockQuantity"), and matching is case-sensitive by default.
// Learned from: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization
// and https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-casing
public class SeedDataLoader
{
    private readonly PosDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedSettings _seedSettings;

    public SeedDataLoader(PosDbContext context, IPasswordHasher passwordHasher, SeedSettings seedSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _seedSettings = seedSettings;
    }

    public async Task LoadAsync()
    {
        SeedDataFile seedDataFile = await ReadSeedFileAsync();

        bool hasCategoriesAlready = await _context.Categories.AnyAsync();
        if (hasCategoriesAlready == false)
        {
            await SaveCategoriesAndProductsAsync(seedDataFile);
        }

        bool hasUsersAlready = await _context.Users.AnyAsync();
        if (hasUsersAlready == false)
        {
            await SaveStaffAsync(seedDataFile);
        }
    }

    private static async Task<SeedDataFile> ReadSeedFileAsync()
    {
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

        return seedDataFile;
    }

    private async Task SaveCategoriesAndProductsAsync(SeedDataFile seedDataFile)
    {
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

    private async Task SaveStaffAsync(SeedDataFile seedDataFile)
    {
        foreach (SeedUser seedUser in seedDataFile.Users)
        {
            UserRole role = Enum.Parse<UserRole>(seedUser.Role);

            string plainPassword = _seedSettings.CashierPassword;
            if (role == UserRole.Admin)
            {
                plainPassword = _seedSettings.AdminPassword;
            }

            User user = new User
            {
                FullName = seedUser.FullName,
                Email = seedUser.Email,
                PasswordHash = _passwordHasher.HashPassword(plainPassword),
                Role = role,
                RegisteredAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }
        await _context.SaveChangesAsync();
    }
}