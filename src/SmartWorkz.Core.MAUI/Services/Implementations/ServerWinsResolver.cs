namespace SmartWorkz.Mobile.Services.Implementations;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Resolves conflicts by always selecting the remote (server) change.
/// </summary>
public class ServerWinsResolver : IConflictResolver
{
    private readonly ILogger? _logger;

    public ServerWinsResolver(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Task<SyncChange> ResolveAsync(SyncConflict conflict)
    {
        Guard.NotNull(conflict, nameof(conflict));
        _logger?.LogDebug("ServerWins: {Winner}", conflict.RemoteChange.DisplayName);
        return Task.FromResult(conflict.RemoteChange);
    }

    public bool CanResolve(ConflictResolutionStrategy strategy) =>
        strategy == ConflictResolutionStrategy.ServerWins;
}
