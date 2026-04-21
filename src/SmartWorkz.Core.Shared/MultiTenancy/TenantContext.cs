using SmartWorkz.Core.Shared.Guards;

namespace SmartWorkz.Shared;

/// <summary>
/// Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
///
/// AsyncLocal ensures:
/// - Thread-safe storage per async execution context
/// - Isolation between concurrent requests (each gets its own context)
/// - Proper inheritance to child tasks (when awaited)
///
/// Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private readonly AsyncLocal<string> _tenantId = new() { Value = "default" };

    /// <summary>
    /// Gets or sets the current tenant identifier.
    /// </summary>
    public string TenantId
    {
        get => _tenantId.Value ?? "default";
        set => _tenantId.Value = Guard.NotEmpty(value, nameof(value));
    }

    /// <summary>
    /// Gets a value indicating whether the current context is multi-tenant.
    /// Always returns true for TenantContext.
    /// </summary>
    public bool IsMultiTenant => true;

    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    /// <returns>The current tenant ID, or "default" if not set.</returns>
    public string GetTenantId() => TenantId;

    /// <summary>
    /// Sets the current tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant ID to set. Cannot be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown if tenantId is null, empty, or whitespace.</exception>
    public void SetTenantId(string tenantId)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        TenantId = tenantId;
    }
}
