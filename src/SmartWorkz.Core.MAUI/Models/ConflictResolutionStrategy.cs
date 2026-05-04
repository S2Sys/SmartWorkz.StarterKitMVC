namespace SmartWorkz.Mobile.Models;

/// <summary>
/// Strategy for resolving sync conflicts between local and remote changes.
/// </summary>
public enum ConflictResolutionStrategy
{
    /// <summary>The most recent change wins (based on timestamp).</summary>
    LastWriteWins = 1,

    /// <summary>Local changes always win over remote.</summary>
    ClientWins = 2,

    /// <summary>Remote changes always win over local.</summary>
    ServerWins = 3,

    /// <summary>Custom resolver decides the outcome.</summary>
    CustomResolver = 4,
}
