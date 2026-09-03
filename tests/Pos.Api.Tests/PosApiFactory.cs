using Microsoft.AspNetCore.Mvc.Testing;

namespace Pos.Api.Tests;

// Runs the real API in memory against its own test database. Follows the Microsoft docs page "Integration tests in ASP.NET Core".
// Settings are environment variables, the same source Docker uses: they are read after user-secrets and win; values from ConfigureAppConfiguration were read before user-secrets and lost.
public class PosApiFactory : WebApplicationFactory<Program>
{
    public PosApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__PosDatabase", @"Server=(localdb)\MSSQLLocalDB;Database=PosDbTest;Trusted_Connection=True;TrustServerCertificate=True");
        Environment.SetEnvironmentVariable("Jwt__Secret", "test-secret-that-is-at-least-thirty-two-characters-long");
        Environment.SetEnvironmentVariable("Seed__AdminPassword", "Admin#Test2026");
        Environment.SetEnvironmentVariable("Seed__CashierPassword", "Cashier#Test2026");
        Environment.SetEnvironmentVariable("RabbitMq__Host", "localhost");
    }
}