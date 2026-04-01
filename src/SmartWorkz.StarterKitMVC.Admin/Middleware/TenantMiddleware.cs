namespace SmartWorkz.StarterKitMVC.Admin.Middleware;

/// <summary>
/// Resolves TenantId once per request and stores it in HttpContext.Items["TenantId"].
/// Resolution order:
///   1. Authenticated user's "tenant" claim
///   2. X-Tenant-ID request header
///   3. Subdomain
///   4. Falls back to "DEFAULT"
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Items["TenantId"] = ResolveTenantId(context);
        await _next(context);
    }

    private static string ResolveTenantId(HttpContext context)
    {
        var claim = context.User.FindFirst("tenant")?.Value
                 ?? context.User.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrWhiteSpace(claim)) return claim;

        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var header)
            && !string.IsNullOrWhiteSpace(header))
            return header.ToString();

        var host  = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length >= 3 && parts[0] != "www")
            return parts[0];

        return "DEFAULT";
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantMiddleware>();
}
