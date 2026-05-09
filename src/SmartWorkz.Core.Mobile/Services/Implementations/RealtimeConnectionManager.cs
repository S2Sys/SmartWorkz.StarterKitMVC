namespace SmartWorkz.Mobile;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

/// <summary>
/// Manages real-time connection lifecycle, reconnection, and health checks.
/// Ensures the connection to the SignalR hub remains active with automatic recovery.
/// </summary>
public class RealtimeConnectionManager
{
    private readonly IRealtimeService _realtimeService;
    private readonly ILogger<RealtimeConnectionManager>? _logger;
    private string? _currentUserId;
    private Timer? _healthCheckTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealtimeConnectionManager"/> class.
    /// </summary>
    /// <param name="realtimeService">The underlying real-time service.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public RealtimeConnectionManager(
        IRealtimeService realtimeService,
        ILogger<RealtimeConnectionManager>? logger = null)
    {
        _realtimeService = Guard.NotNull(realtimeService, nameof(realtimeService));
        _logger = logger;
    }

    /// <summary>
    /// Ensures connection is active, reconnecting if necessary.
    /// </summary>
    /// <param name="userId">The user ID to connect as.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure of the connection attempt.</returns>
    public async Task<Result> EnsureConnectedAsync(
        string userId,
        CancellationToken ct = default)
    {
        Guard.NotEmpty(userId, nameof(userId));

        _currentUserId = userId;
        var isConnected = await _realtimeService.IsConnectedAsync();

        if (isConnected)
        {
            _logger?.LogDebug("Real-time connection already active for user {UserId}", userId);
            return Result.Ok();
        }

        _logger?.LogInformation("Establishing real-time connection for user {UserId}", userId);
        var result = await _realtimeService.ConnectAsync(userId, ct);

        if (result.Succeeded)
        {
            StartHealthCheck();
            _logger?.LogInformation("Real-time connection established successfully for user {UserId}", userId);
        }
        else
        {
            _logger?.LogError("Failed to establish real-time connection for user {UserId}", userId);
        }

        return result;
    }

    /// <summary>
    /// Gracefully disconnect and clean up resources.
    /// </summary>
    /// <returns>A result indicating success or failure of the disconnection.</returns>
    public async Task<Result> DisconnectAsync()
    {
        _logger?.LogInformation("Disconnecting real-time service");
        StopHealthCheck();
        _currentUserId = null;
        return await _realtimeService.DisconnectAsync();
    }

    /// <summary>
    /// Perform health check and reconnect if the connection is lost.
    /// </summary>
    /// <returns>True if the connection is healthy, false otherwise.</returns>
    public async Task<bool> HealthCheckAsync()
    {
        if (string.IsNullOrEmpty(_currentUserId))
        {
            return false;
        }

        var isConnected = await _realtimeService.IsConnectedAsync();

        if (!isConnected)
        {
            _logger?.LogWarning("Health check failed: connection lost for user {UserId}. Attempting reconnect...", _currentUserId);
            var result = await _realtimeService.ConnectAsync(_currentUserId);

            if (!result.Succeeded)
            {
                _logger?.LogError("Reconnection attempt failed for user {UserId}", _currentUserId);
            }

            return result.Succeeded;
        }

        return true;
    }

    /// <summary>
    /// Start periodic health checks.
    /// </summary>
    private void StartHealthCheck()
    {
        StopHealthCheck();
        _healthCheckTimer = new Timer(
            async _ => await HealthCheckAsync(),
            null,
            TimeSpan.FromSeconds(30),  // First check after 30 seconds
            TimeSpan.FromSeconds(60)); // Then every 60 seconds

        _logger?.LogDebug("Health check timer started");
    }

    /// <summary>
    /// Stop health check timer.
    /// </summary>
    private void StopHealthCheck()
    {
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        _logger?.LogDebug("Health check timer stopped");
    }
}
