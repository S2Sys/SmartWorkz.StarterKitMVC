namespace SmartWorkz.Mobile.Services.Implementations;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Resolves conflicts by using the most recent change (by timestamp).
/// </summary>
public class LastWriteWinsResolver : IConflictResolver
{
    private readonly ILogger? _logger;

    public LastWriteWinsResolver(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Task<SyncChange> ResolveAsync(SyncConflict conflict)
    {
        Guard.NotNull(conflict, nameof(conflict));

        var winner = conflict.LocalChange.Timestamp > conflict.RemoteChange.Timestamp
            ? conflict.LocalChange
            : conflict.RemoteChange;

        _logger?.LogDebug("LastWriteWins: {Winner} (timestamp: {Timestamp})",
            winner.DisplayName, winner.Timestamp);

        return Task.FromResult(winner);
    }

    public bool CanResolve(ConflictResolutionStrategy strategy) =>
        strategy == ConflictResolutionStrategy.LastWriteWins;
}
