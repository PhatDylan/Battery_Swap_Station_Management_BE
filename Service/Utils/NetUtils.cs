using Microsoft.AspNetCore.Http;

namespace Service.Utils;

public class NetUtils
{
    public static string GenerateCallbackUrl(IHttpContextAccessor httpContextAccessor, string uri)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            throw new InvalidOperationException("No active HTTP request context.");
        }

        var scheme = request.Scheme;
        var host = request.Host.Host;
        var port = request.Host.Port ?? (scheme == "https" ? 443 : 80);
        var baseUrl = port is 80 or 443
            ? $"{scheme}://{host}"
            : $"{scheme}://{host}:{port}";

        var pathBase = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;

        return $"{baseUrl}{pathBase}{uri}";
    }
}