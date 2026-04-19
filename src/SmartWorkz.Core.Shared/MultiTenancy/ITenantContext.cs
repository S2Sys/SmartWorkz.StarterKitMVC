namespace SmartWorkz.Core.Shared.MultiTenancy;

/// <summary>
/// Scoped service providing current tenant ID for multi-tenant applications.
/// Resolved from request context or claims principal.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    /// <returns>Tenant ID, or null if operating in single-tenant context.</returns>
    string? GetTenantId();

    /// <summary>
    /// Sets the current tenant identifier (rarely used; typically set from request context).
    /// </summary>
    /// <param name="tenantId">Tenant ID to set.</param>
    void SetTenantId(string tenantId);

    /// <summary>
    /// Gets a value indicating whether the current context is multi-tenant.
    /// </summary>
    bool IsMultiTenant { get; }
}
