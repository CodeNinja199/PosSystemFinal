namespace Pos.Infrastructure.SeedData;

// The whole of seed-data.json, deserialized by SeedDataLoader.
public class SeedDataFile
{
    public List<SeedCategory> Categories { get; set; } = new List<SeedCategory>();

    public List<SeedProduct> Products { get; set; } = new List<SeedProduct>();

    public List<SeedUser> Users { get; set; } = new List<SeedUser>();
}