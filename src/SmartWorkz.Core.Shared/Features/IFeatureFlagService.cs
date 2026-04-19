namespace SmartWorkz.Core.Shared.Features;

/// <summary>
/// Global feature flag service for cross-tenant feature control.
/// Use for organization-wide feature toggles (not tenant-specific).
/// For tenant-scoped flags, use ITenantFeatureFlags.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a global feature is enabled.
    /// </summary>
    /// <param name="flagName">Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if feature is enabled globally.</returns>
    Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all enabled global features.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of enabled feature flag names.</returns>
    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default);
}
