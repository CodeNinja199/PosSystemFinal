using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Pos.Api.Tests;

// Runs the real API in memory against its own test database. Follows the Microsoft docs page "Integration tests in ASP.NET Core".
// The settings below replace user-secrets, so the tests need only LocalDB and nothing from the developer's profile.
public class PosApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Dictionary<string, string?> testSettings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:PosDatabase"] = @"Server=(localdb)\MSSQLLocalDB;Database=PosDbTest;Trusted_Connection=True;TrustServerCertificate=True",
            ["Jwt:Secret"] = "test-secret-that-is-at-least-thirty-two-characters-long",
            ["Seed:AdminPassword"] = "Admin#Test2026",
            ["Seed:CashierPassword"] = "Cashier#Test2026"
        };

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(testSettings);
        });
    }
}