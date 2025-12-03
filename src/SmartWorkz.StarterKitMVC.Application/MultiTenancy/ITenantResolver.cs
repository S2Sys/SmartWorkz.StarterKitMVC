namespace SmartWorkz.StarterKitMVC.Application.MultiTenancy;

/// <summary>
/// Resolves the tenant ID from the current HTTP request.
/// </summary>
/// <example>
/// <code>
/// // Implement custom resolver
/// public class SubdomainTenantResolver : ITenantResolver
/// {
///     public Task&lt;string?&gt; ResolveAsync(HttpContext context, CancellationToken ct)
///     {
///         var host = context.Request.Host.Host;
///         var subdomain = host.Split('.').FirstOrDefault();
///         return Task.FromResult(subdomain);
///     }
/// }
/// </code>
/// </example>
public interface ITenantResolver
{
    /// <summary>
    /// Resolves the tenant ID from the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved tenant ID or null.</returns>
    Task<string?> ResolveAsync(HttpContext context, CancellationToken ct = default);
}
