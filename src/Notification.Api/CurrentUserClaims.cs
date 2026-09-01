using System.Security.Claims;

namespace Notification.Api;

// Reads the user id claim the POS API put into the token. A copy of the one method this service needs.
public static class CurrentUserClaims
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        Claim? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new InvalidOperationException("The token has no user id claim.");
        }

        int userId = int.Parse(userIdClaim.Value);

        return userId;
    }
}