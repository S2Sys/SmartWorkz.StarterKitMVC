namespace SmartWorkz.Mobile.Services;

using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;

/// <summary>
/// Resolves sync conflicts using various strategies.
/// </summary>
public interface IConflictResolver
{
    /// <summary>
    /// Resolve a single conflict.
    /// </summary>
    Task<SyncChange> ResolveAsync(SyncConflict conflict);

    /// <summary>
    /// Check if this resolver handles the given strategy.
    /// </summary>
    bool CanResolve(ConflictResolutionStrategy strategy);
}
