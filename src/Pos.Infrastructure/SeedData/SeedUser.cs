namespace Pos.Infrastructure.SeedData;

// One entry of the "users" array in seed-data.json. The password comes from configuration by role, never from the file.
public class SeedUser
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}