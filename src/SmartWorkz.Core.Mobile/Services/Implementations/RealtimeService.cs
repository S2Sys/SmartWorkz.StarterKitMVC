namespace SmartWorkz.Mobile.Services.Implementations;

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// SignalR-based real-time communication service implementation.
/// </summary>
public class RealtimeService : IRealtimeService, IDisposable
{
    private readonly string _hubUrl;
    private readonly ILogger<RealtimeService> _logger;
    private HubConnection? _hubConnection;

    private readonly Subject<RealtimeMessage> _messageSubject = new();
    private readonly Subject<RealtimeConnectionState> _connectionStateSubject = new();
    private RealtimeConnectionState _currentState = RealtimeConnectionState.Disconnected;
    private string? _currentUserId;

    public RealtimeService(string hubUrl, ILogger<RealtimeService> logger)
    {
        _hubUrl = Guard.NotEmpty(hubUrl, nameof(hubUrl));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Result> ConnectAsync(string userId, CancellationToken ct = default)
    {
        Guard.NotEmpty(userId, nameof(userId));

        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
                return Result.Ok();

            _currentUserId = userId;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30),
                })
                .WithServerTimeout(TimeSpan.FromSeconds(30))
                .Build();

            RegisterHandlers();

            await SetConnectionStateAsync(RealtimeConnectionState.Connecting);
            await _hubConnection.StartAsync(ct);

            await _hubConnection.InvokeAsync("SetUserId", userId, cancellationToken: ct);
            await SetConnectionStateAsync(RealtimeConnectionState.Connected);

            _logger.LogInformation("Connected to real-time hub for user {UserId}", userId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            await SetConnectionStateAsync(RealtimeConnectionState.Error);
            _logger.LogError(ex, "Failed to connect to real-time hub");
            return Result.Fail(Error.FromException(ex, "REALTIME_CONNECTION_FAILED"));
        }
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_hubConnection?.State == HubConnectionState.Connected);
    }

    public IObservable<RealtimeMessage> OnMessageReceived()
    {
        return _messageSubject;
    }

    public async Task<Result> SendAsync(string method, object[] args, CancellationToken ct = default)
    {
        Guard.NotEmpty(method, nameof(method));

        try
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
                return Result.Fail("Service not connected");

            await _hubConnection.InvokeAsync(method, args, cancellationToken: ct);
            _logger.LogDebug("Sent message to hub method {Method}", method);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to hub");
            return Result.Fail(Error.FromException(ex, "REALTIME_SEND_FAILED"));
        }
    }

    public async Task<Result> SubscribeToAsync(string channel, CancellationToken ct = default)
    {
        Guard.NotEmpty(channel, nameof(channel));

        try
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
                return Result.Fail("Service not connected");

            await _hubConnection.InvokeAsync("SubscribeToChannel", channel, cancellationToken: ct);
            _logger.LogInformation("Subscribed to channel {Channel}", channel);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to channel {Channel}", channel);
            return Result.Fail(Error.FromException(ex, "SUBSCRIPTION_FAILED"));
        }
    }

    public async Task<Result> UnsubscribeFromAsync(string channel, CancellationToken ct = default)
    {
        Guard.NotEmpty(channel, nameof(channel));

        try
        {
            if (_hubConnection?.State != HubConnectionState.Connected)
                return Result.Fail("Service not connected");

            await _hubConnection.InvokeAsync("UnsubscribeFromChannel", channel, cancellationToken: ct);
            _logger.LogInformation("Unsubscribed from channel {Channel}", channel);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from channel {Channel}", channel);
            return Result.Fail(Error.FromException(ex, "UNSUBSCRIBE_FAILED"));
        }
    }

    public async Task<Result> DisconnectAsync()
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
            }

            await SetConnectionStateAsync(RealtimeConnectionState.Disconnected);
            _logger.LogInformation("Disconnected from real-time hub");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from hub");
            return Result.Fail(Error.FromException(ex, "DISCONNECT_FAILED"));
        }
    }

    public RealtimeConnectionState GetConnectionState() => _currentState;

    public IObservable<RealtimeConnectionState> OnConnectionStateChanged()
        => _connectionStateSubject;

    private void RegisterHandlers()
    {
        Guard.NotNull(_hubConnection, nameof(_hubConnection));

        _hubConnection!.On<string, string, string>("ReceiveMessage",
            (channel, method, payload) =>
        {
            _logger.LogDebug("Received message on {Channel}/{Method}", channel, method);

            var message = new RealtimeMessage(
                Channel: channel,
                Method: method,
                Payload: payload,
                ReceivedAt: DateTime.UtcNow,
                CorrelationId: Guid.NewGuid().ToString());

            _messageSubject.OnNext(message);
        });

        _hubConnection!.Reconnecting += OnReconnecting;
        _hubConnection!.Reconnected += OnReconnected;
        _hubConnection!.Closed += OnClosed;
    }

    private async Task OnReconnecting(Exception? error)
    {
        _logger.LogWarning(error, "Reconnecting to hub");
        await SetConnectionStateAsync(RealtimeConnectionState.Reconnecting);
    }

    private async Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("Reconnected to hub with connection {ConnectionId}", connectionId);
        await SetConnectionStateAsync(RealtimeConnectionState.Connected);
    }

    private async Task OnClosed(Exception? error)
    {
        _logger.LogWarning(error, "Hub connection closed");
        await SetConnectionStateAsync(RealtimeConnectionState.Disconnected);
    }

    private async Task SetConnectionStateAsync(RealtimeConnectionState state)
    {
        if (_currentState != state)
        {
            _currentState = state;
            _connectionStateSubject.OnNext(state);
            await Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _hubConnection?.DisposeAsync().GetAwaiter().GetResult();
        _messageSubject?.Dispose();
        _connectionStateSubject?.Dispose();
    }
}
