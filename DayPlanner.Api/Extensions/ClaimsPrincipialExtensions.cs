using System.Security.Claims;

namespace DayPlanner.Api.Extensions;

internal static class ClaimsPrincipialExtensions
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);
        return claimsPrincipal.Claims.SingleOrDefault(claim => claim.Type == "user_id")?.Value;
    }
}
