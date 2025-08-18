using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Aurora.Coinly.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        string? userId = claimsPrincipal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

        if (!Guid.TryParse(userId, out Guid parsedUserId))
        {
            throw new AuroraCoinlyException("User identifier is not valid.");
        }

        return parsedUserId;
    }

    public static string GetIdentityId(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            throw new AuroraCoinlyException("User identity is unavailable");
    }
}