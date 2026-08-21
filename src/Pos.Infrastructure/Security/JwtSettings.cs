namespace Pos.Infrastructure.Security;

// The Jwt section of configuration, bound once in Program.cs and registered as a singleton: settings never change while the app runs.
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}