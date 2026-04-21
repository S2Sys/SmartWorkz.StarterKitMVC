using System.Collections.Concurrent;

namespace SmartWorkz.Shared;

/// <summary>
/// In-memory feature flag provider for tenant-scoped feature control.
///
/// Uses ConcurrentDictionary to store tenant-specific flags:
/// - Key: tenant ID
/// - Value: HashSet of enabled feature flag names
///
/// Thread-safe for concurrent operations. Suitable for in-process caching
/// or dev/test scenarios. For distributed systems, integrate with a
/// centralized feature flag service (Unleash, LaunchDarkly, etc.).
/// </summary>
public sealed class DefaultTenantFeatureFlags : ITenantFeatureFlags
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _tenantFlags = new();

    /// <summary>
    /// Checks if a feature is enabled for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="flagName">The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Task containing true if the feature is enabled for this tenant,
    /// false if the tenant doesn't exist or the flag is not enabled.
    /// </returns>
    public Task<bool> IsEnabledAsync(string tenantId, string flagName, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        var isEnabled = _tenantFlags.TryGetValue(tenantId, out var flags) && flags.Contains(flagName);
        return Task.FromResult(isEnabled);
    }

    /// <summary>
    /// Gets all enabled features for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Task containing a read-only list of enabled feature flag names.
    /// Returns an empty list if the tenant doesn't exist or has no enabled flags.
    /// </returns>
    public Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyList<string>>(cancellationToken);
        }

        var features = _tenantFlags.TryGetValue(tenantId, out var flags)
            ? flags.ToList().AsReadOnly()
            : (IReadOnlyList<string>)[];

        return Task.FromResult(features);
    }

    /// <summary>
    /// Enables a feature flag for a tenant.
    /// Idempotent — calling multiple times with the same flag is safe.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="flagName">The feature flag name.</param>
    public void EnableFlag(string tenantId, string flagName)
    {
        _tenantFlags.AddOrUpdate(
            tenantId,
            new HashSet<string> { flagName },
            (_, flags) =>
            {
                flags.Add(flagName);
                return flags;
            });
    }

    /// <summary>
    /// Disables a feature flag for a tenant.
    /// Idempotent — calling multiple times with the same flag is safe.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="flagName">The feature flag name.</param>
    public void DisableFlag(string tenantId, string flagName)
    {
        if (_tenantFlags.TryGetValue(tenantId, out var flags))
        {
            flags.Remove(flagName);
            // Optionally remove tenant entry if no flags remain (not required for functionality)
        }
    }
}
