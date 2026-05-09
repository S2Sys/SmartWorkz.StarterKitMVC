namespace SmartWorkz.Mobile.Services;

using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Automatic reconnection service with exponential backoff.
/// </summary>
public interface IAutoReconnectService
{
    /// <summary>
    /// Start auto-reconnect monitoring.
    /// </summary>
    /// <param name="userId">The user ID to reconnect as.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> StartAsync(string userId);

    /// <summary>
    /// Stop auto-reconnect monitoring.
    /// </summary>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> StopAsync();

    /// <summary>
    /// Check if currently reconnecting.
    /// </summary>
    bool IsReconnecting { get; }

    /// <summary>
    /// Get reconnection statistics.
    /// </summary>
    /// <returns>Result containing reconnection statistics.</returns>
    Task<Result<ReconnectStats>> GetStatsAsync();

    /// <summary>
    /// Force immediate reconnection attempt.
    /// </summary>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ReconnectImmediatelyAsync();
}
