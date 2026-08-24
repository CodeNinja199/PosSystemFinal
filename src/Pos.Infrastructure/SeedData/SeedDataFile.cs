namespace Pos.Infrastructure.SeedData;

// The whole of seed-data.json, deserialized by SeedDataLoader.
public class SeedDataFile
{
    public List<SeedStore> Stores { get; set; } = new List<SeedStore>();
}