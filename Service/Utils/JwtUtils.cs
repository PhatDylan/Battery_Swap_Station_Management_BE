using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Service.Utils;

public class JwtUtils
{
    public static string? GetUserId(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
            return null;

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value;
    }
}