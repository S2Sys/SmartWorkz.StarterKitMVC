namespace SmartWorkz.Mobile.Services.Implementations;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Resolves conflicts by always selecting the local (client) change.
/// </summary>
public class ClientWinsResolver : IConflictResolver
{
    private readonly ILogger? _logger;

    public ClientWinsResolver(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Task<SyncChange> ResolveAsync(SyncConflict conflict)
    {
        Guard.NotNull(conflict, nameof(conflict));
        _logger?.LogDebug("ClientWins: {Winner}", conflict.LocalChange.DisplayName);
        return Task.FromResult(conflict.LocalChange);
    }

    public bool CanResolve(ConflictResolutionStrategy strategy) =>
        strategy == ConflictResolutionStrategy.ClientWins;
}
