using System.Security.Claims;

using Pos.Domain.Enums;
using Pos.Infrastructure.Security;

namespace Pos.Api;

// Reads the four claims JwtLoginTokenCreator put into the token, after the JWT bearer middleware has turned it into User.
// Called by the controllers, which pass the values on to the services as plain parameters.
public static class CurrentUserClaims
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        string userIdFromToken = GetClaimValue(user, ClaimTypes.NameIdentifier);

        int userId = int.Parse(userIdFromToken);

        return userId;
    }

    public static UserRole GetRole(ClaimsPrincipal user)
    {
        string roleFromToken = GetClaimValue(user, ClaimTypes.Role);

        UserRole role = Enum.Parse<UserRole>(roleFromToken);

        return role;
    }

    public static int GetStoreId(ClaimsPrincipal user)
    {
        string storeIdFromToken = GetClaimValue(user, JwtLoginTokenCreator.StoreIdClaimName);

        int storeId = int.Parse(storeIdFromToken);

        return storeId;
    }

    private static string GetClaimValue(ClaimsPrincipal user, string claimType)
    {
        Claim? claim = user.FindFirst(claimType);
        if (claim == null)
        {
            throw new InvalidOperationException($"The token has no {claimType} claim.");
        }

        return claim.Value;
    }
}