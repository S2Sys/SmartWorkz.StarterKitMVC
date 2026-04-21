using SmartWorkz.Core.Shared.Guards;
using System.Collections.Concurrent;

namespace SmartWorkz.Shared;

/// <summary>
/// Global (non-tenant) feature flag service with in-memory storage.
/// Thread-safe implementation suitable for single-process deployments.
/// Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.
/// </summary>
public sealed class DefaultFeatureFlagService : IFeatureFlagService
{
    private readonly ConcurrentDictionary<string, bool> _flags = new();

    /// <summary>
    /// Checks if a feature flag is enabled.
    /// Returns false for unknown flags (does not throw).
    /// </summary>
    /// <param name="flagName">The name of the feature flag to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the flag exists and is enabled; false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when flagName is null, empty, or whitespace.</exception>
    public async Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(flagName, nameof(flagName));

        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<bool>(cancellationToken);

        return await Task.FromResult(_flags.TryGetValue(flagName, out var enabled) ? enabled : false);
    }

    /// <summary>
    /// Gets all enabled feature flags.
    /// Returns empty list if no flags are enabled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of enabled feature flag names.</returns>
    public async Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<IReadOnlyList<string>>(cancellationToken);

        var enabled = _flags
            .Where(x => x.Value)
            .Select(x => x.Key)
            .ToList();

        return await Task.FromResult((IReadOnlyList<string>)enabled);
    }

    /// <summary>
    /// Enables a feature flag.
    /// Creates the flag if it doesn't exist.
    /// </summary>
    /// <param name="flagName">The name of the feature flag to enable.</param>
    /// <exception cref="ArgumentException">Thrown when flagName is null, empty, or whitespace.</exception>
    public void EnableFlag(string flagName)
    {
        Guard.NotEmpty(flagName, nameof(flagName));
        _flags[flagName] = true;
    }

    /// <summary>
    /// Disables a feature flag.
    /// Creates the flag as disabled if it doesn't exist.
    /// </summary>
    /// <param name="flagName">The name of the feature flag to disable.</param>
    /// <exception cref="ArgumentException">Thrown when flagName is null, empty, or whitespace.</exception>
    public void DisableFlag(string flagName)
    {
        Guard.NotEmpty(flagName, nameof(flagName));
        _flags[flagName] = false;
    }
}
