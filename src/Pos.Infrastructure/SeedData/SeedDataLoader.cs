using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Enums;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.SeedData;

// How the seed loader works here:
// 1. Program.cs asks for this class once at startup and calls LoadAsync before the API listens; if the Stores table has rows, nothing happens.
// 2. Otherwise it reads seed-data.json and deserializes it (JSON text -> C# objects; serialize is the other direction, C# object -> JSON text).
// 3. For each store it saves the store first so it gets an id, then its categories, then its products, then its staff with the Seed passwords, hashed.
// PropertyNameCaseInsensitive is set because the file is camelCase ("stockQuantity") and the classes are PascalCase; matching is case-sensitive by default.
// Learned from: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization and .../system-text-json/character-casing
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
        bool hasStoresAlready = await _context.Stores.AnyAsync();
        if (hasStoresAlready)
        {
            return;
        }

        SeedDataFile seedDataFile = await ReadSeedFileAsync();

        foreach (SeedStore seedStore in seedDataFile.Stores)
        {
            Store store = new Store
            {
                Name = seedStore.Name
            };
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            await SaveCategoriesAndProductsAsync(seedStore, store.Id);
            await SaveStaffAsync(seedStore, store.Id);
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

    private async Task SaveCategoriesAndProductsAsync(SeedStore seedStore, int storeId)
    {
        Dictionary<string, Category> categoriesByName = new Dictionary<string, Category>();
        foreach (SeedCategory seedCategory in seedStore.Categories)
        {
            Category category = new Category
            {
                StoreId = storeId,
                Name = seedCategory.Name
            };
            _context.Categories.Add(category);
            categoriesByName.Add(seedCategory.Name, category);
        }
        await _context.SaveChangesAsync();

        foreach (SeedProduct seedProduct in seedStore.Products)
        {
            Category categoryForProduct = categoriesByName[seedProduct.CategoryName];
            Product product = new Product
            {
                StoreId = storeId,
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

    private async Task SaveStaffAsync(SeedStore seedStore, int storeId)
    {
        foreach (SeedUser seedUser in seedStore.Users)
        {
            UserRole role = Enum.Parse<UserRole>(seedUser.Role);

            string plainPassword = _seedSettings.CashierPassword;
            if (role == UserRole.Admin)
            {
                plainPassword = _seedSettings.AdminPassword;
            }

            User user = new User
            {
                StoreId = storeId,
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