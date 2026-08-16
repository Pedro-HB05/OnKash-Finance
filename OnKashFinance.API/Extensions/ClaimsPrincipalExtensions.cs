using System.Security.Claims;

namespace OnKashFinance.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(claim))
            return null;

        return Guid.TryParse(claim, out var userId)
            ? userId
            : null;
    }
}