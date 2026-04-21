namespace SmartWorkz.Shared;

/// <summary>
/// Feature flag provider scoped to a specific tenant.
/// Allows per-tenant feature control.
/// </summary>
public interface ITenantFeatureFlags
{
    /// <summary>
    /// Checks if a feature is enabled for the specified tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID.</param>
    /// <param name="flagName">Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if feature is enabled for this tenant.</returns>
    Task<bool> IsEnabledAsync(string tenantId, string flagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all enabled features for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of enabled feature flag names.</returns>
    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(string tenantId, CancellationToken cancellationToken = default);
}
