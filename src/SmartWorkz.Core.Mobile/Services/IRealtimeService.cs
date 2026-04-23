namespace SmartWorkz.Mobile.Services;

using System;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// SignalR-based real-time communication service for mobile apps.
/// </summary>
public interface IRealtimeService
{
    /// <summary>
    /// Establishes connection to SignalR hub.
    /// </summary>
    Task<Result> ConnectAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if currently connected to hub.
    /// </summary>
    Task<bool> IsConnectedAsync();

    /// <summary>
    /// Observable stream of incoming messages.
    /// </summary>
    IObservable<RealtimeMessage> OnMessageReceived();

    /// <summary>
    /// Send message to server hub method.
    /// </summary>
    Task<Result> SendAsync(string method, object[] args, CancellationToken ct = default);

    /// <summary>
    /// Subscribe to channel for broadcast messages.
    /// </summary>
    Task<Result> SubscribeToAsync(string channel, CancellationToken ct = default);

    /// <summary>
    /// Unsubscribe from channel.
    /// </summary>
    Task<Result> UnsubscribeFromAsync(string channel, CancellationToken ct = default);

    /// <summary>
    /// Disconnect from hub and clean up resources.
    /// </summary>
    Task<Result> DisconnectAsync();

    /// <summary>
    /// Get current connection state.
    /// </summary>
    RealtimeConnectionState GetConnectionState();

    /// <summary>
    /// Observable of connection state changes.
    /// </summary>
    IObservable<RealtimeConnectionState> OnConnectionStateChanged();
}
