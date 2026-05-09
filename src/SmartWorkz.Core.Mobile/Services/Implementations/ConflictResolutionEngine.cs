namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Engine for resolving sync conflicts using various strategies.
/// </summary>
public class ConflictResolutionEngine
{
    private readonly Dictionary<ConflictResolutionStrategy, IConflictResolver> _resolvers = new();
    private readonly ILogger? _logger;

    public ConflictResolutionEngine(ILogger? logger = null)
    {
        _logger = logger;
        RegisterDefaultResolvers();
    }

    /// <summary>
    /// Resolve a conflict using the configured strategy.
    /// </summary>
    public async Task<SyncChange> ResolveAsync(SyncConflict conflict)
    {
        Guard.NotNull(conflict, nameof(conflict));

        if (!_resolvers.TryGetValue(conflict.ResolutionStrategy, out var resolver))
        {
            _logger?.LogWarning(
                "No resolver for strategy {Strategy}, using LastWriteWins",
                conflict.ResolutionStrategy);
            resolver = _resolvers[ConflictResolutionStrategy.LastWriteWins];
        }

        var resolved = await resolver.ResolveAsync(conflict);
        _logger?.LogInformation(
            "Resolved conflict {ConflictId} for {Entity}: {Resolution}",
            conflict.ConflictId,
            conflict.LocalChange.EntityType,
            conflict.ResolutionStrategy);

        return resolved;
    }

    /// <summary>
    /// Register a custom resolver for a strategy.
    /// </summary>
    public void RegisterResolver(ConflictResolutionStrategy strategy, IConflictResolver resolver)
    {
        Guard.NotNull(resolver, nameof(resolver));
        _resolvers[strategy] = resolver;
        _logger?.LogInformation("Registered resolver for strategy {Strategy}", strategy);
    }

    private void RegisterDefaultResolvers()
    {
        _resolvers[ConflictResolutionStrategy.LastWriteWins] =
            new LastWriteWinsResolver(_logger);
        _resolvers[ConflictResolutionStrategy.ClientWins] =
            new ClientWinsResolver(_logger);
        _resolvers[ConflictResolutionStrategy.ServerWins] =
            new ServerWinsResolver(_logger);
    }
}
