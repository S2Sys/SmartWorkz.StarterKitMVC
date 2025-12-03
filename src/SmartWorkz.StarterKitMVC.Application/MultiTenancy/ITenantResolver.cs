namespace SmartWorkz.StarterKitMVC.Application.MultiTenancy;

/// <summary>
/// Resolves the tenant ID from the current request context.
/// </summary>
/// <example>
/// <code>
/// // Implement custom resolver
/// public class SubdomainTenantResolver : ITenantResolver
/// {
///     public Task&lt;string?&gt; ResolveAsync(TenantResolveContext context, CancellationToken ct)
///     {
///         var subdomain = context.Host?.Split('.').FirstOrDefault();
///         return Task.FromResult(subdomain);
///     }
/// }
/// </code>
/// </example>
public interface ITenantResolver
{
    /// <summary>
    /// Resolves the tenant ID from the request context.
    /// </summary>
    /// <param name="context">The tenant resolve context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved tenant ID or null.</returns>
    Task<string?> ResolveAsync(TenantResolveContext context, CancellationToken ct = default);
}

/// <summary>
/// Context for tenant resolution without HttpContext dependency.
/// </summary>
public record TenantResolveContext(
    string? Host,
    string? Path,
    IDictionary<string, string>? Headers,
    IDictionary<string, string>? QueryParams);
