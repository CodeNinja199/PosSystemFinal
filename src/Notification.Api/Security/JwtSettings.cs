namespace Notification.Api.Security;

// The same Jwt section as the POS API: the token it issues is valid here because Secret, Issuer, and Audience match.
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}