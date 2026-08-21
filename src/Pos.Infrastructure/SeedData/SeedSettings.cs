namespace Pos.Infrastructure.SeedData;

// The Seed section of configuration (user-secrets locally): the passwords of the seeded admin and cashier.
public class SeedSettings
{
    public string AdminPassword { get; set; } = string.Empty;

    public string CashierPassword { get; set; } = string.Empty;
}