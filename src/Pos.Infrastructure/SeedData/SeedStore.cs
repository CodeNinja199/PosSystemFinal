namespace Pos.Infrastructure.SeedData;

// One entry of the "stores" array in seed-data.json: the store and everything that belongs to it.
public class SeedStore
{
    public string Name { get; set; } = string.Empty;

    public List<SeedUser> Users { get; set; } = new List<SeedUser>();

    public List<SeedCategory> Categories { get; set; } = new List<SeedCategory>();

    public List<SeedProduct> Products { get; set; } = new List<SeedProduct>();
}