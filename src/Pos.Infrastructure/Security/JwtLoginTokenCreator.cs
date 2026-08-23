using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Security;

// How login tokens work here:
// 1. After the password matches, AuthService asks this class for a token holding the user id, email, and role, signed with Jwt:Secret (HS256).
// 2. The web app keeps the token in an httpOnly cookie and sends it as "Authorization: Bearer ..." on every API call.
// 3. Every API checks the signature, issuer, audience, and expiry with the same Jwt settings before any [Authorize] action runs.
// JsonWebTokenHandler is the handler ASP.NET Core itself uses since version 8; the token is valid for 24 hours.
// Learned from: https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.jsonwebtokens.jsonwebtokenhandler.createtoken
// and https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/8.0/securitytoken-events
public class JwtLoginTokenCreator : ILoginTokenCreator
{
    private readonly JwtSettings _jwtSettings;

    public JwtLoginTokenCreator(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string CreateLoginToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        byte[] secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(secretBytes);
        SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = signingCredentials
        };

        JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
        string token = tokenHandler.CreateToken(tokenDescriptor);

        return token;
    }
}