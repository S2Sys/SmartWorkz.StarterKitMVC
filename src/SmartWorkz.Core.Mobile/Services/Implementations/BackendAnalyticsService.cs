namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWorkz.Core.Shared.Resilience;
using System.Collections.Generic;
using System.Linq;

/// <summary>Sends analytics events to the backend API with rate limiting.</summary>
internal class BackendAnalyticsService : IAnalyticsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRateLimiter _rateLimiter;
    private readonly IMobileContext _mobileContext;
    private readonly ILogger _logger;

    private string? _userId;
    private Dictionary<string, string> _userProperties = [];

    public BackendAnalyticsService(
        IServiceScopeFactory scopeFactory,
        IRateLimiter rateLimiter,
        IMobileContext mobileContext,
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _rateLimiter = rateLimiter;
        _mobileContext = mobileContext;
        _logger = logger;
    }

    public async Task TrackEventAsync(
        string eventName,
        Dictionary<string, object>? properties = null,
        CancellationToken ct = default)
    {
        try
        {
            var rateLimitResult = await _rateLimiter.TryAcquireAsync("analytics", 1, ct);
            if (!rateLimitResult.Succeeded || !rateLimitResult.Data)
            {
                _logger.LogDebug("Analytics rate limit exceeded, event dropped: {EventName}", eventName);
                return;
            }

            var payload = new TelemetryEventPayload(
                EventName: eventName,
                EventType: "Event",
                UserId: _userId,
                Properties: properties,
                UserProperties: _userProperties.Count > 0 ? _userProperties : null,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            var result = await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to send telemetry event {EventName}: {ErrorCode}",
                    eventName, result.Error?.Code ?? "UNKNOWN");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending telemetry event {EventName}", eventName);
        }
    }

    public async Task TrackPageViewAsync(
        string pageName,
        Dictionary<string, object>? properties = null,
        CancellationToken ct = default)
    {
        try
        {
            var rateLimitResult = await _rateLimiter.TryAcquireAsync("analytics", 1, ct);
            if (!rateLimitResult.Succeeded || !rateLimitResult.Data)
            {
                _logger.LogDebug("Analytics rate limit exceeded, page view dropped: {PageName}", pageName);
                return;
            }

            var pageProperties = new Dictionary<string, object>(properties ?? [])
            {
                { "pageName", pageName }
            };

            var payload = new TelemetryEventPayload(
                EventName: $"PageView_{pageName}",
                EventType: "PageView",
                UserId: _userId,
                Properties: pageProperties,
                UserProperties: _userProperties.Count > 0 ? _userProperties : null,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            var result = await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to send page view event {PageName}: {ErrorCode}",
                    pageName, result.Error?.Code ?? "UNKNOWN");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending page view event {PageName}", pageName);
        }
    }

    public async Task TrackErrorAsync(
        Exception ex,
        Dictionary<string, object>? properties = null,
        CancellationToken ct = default)
    {
        try
        {
            var rateLimitResult = await _rateLimiter.TryAcquireAsync("analytics", 1, ct);
            if (!rateLimitResult.Succeeded || !rateLimitResult.Data)
            {
                _logger.LogDebug("Analytics rate limit exceeded, error event dropped: {ExceptionMessage}", ex.Message);
                return;
            }

            var errorProperties = new Dictionary<string, object>(properties ?? [])
            {
                { "exceptionType", ex.GetType().FullName ?? "Unknown" },
                { "exceptionMessage", ex.Message },
                { "stackTrace", ex.StackTrace ?? "N/A" }
            };

            var payload = new TelemetryEventPayload(
                EventName: $"Error_{ex.GetType().Name}",
                EventType: "Error",
                UserId: _userId,
                Properties: errorProperties,
                UserProperties: _userProperties.Count > 0 ? _userProperties : null,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            var result = await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to send error event {ExceptionType}: {ErrorCode}",
                    ex.GetType().Name, result.Error?.Code ?? "UNKNOWN");
            }
        }
        catch (Exception trackingError)
        {
            _logger.LogError(trackingError, "Unexpected error sending error telemetry for {ExceptionType}", ex.GetType().Name);
        }
    }

    public void SetUserId(string userId)
    {
        _userId = userId;
    }

    public void SetUserProperty(string key, string value)
    {
        _userProperties[key] = value;
    }

    public async Task ClearUserPropertiesAsync(CancellationToken ct = default)
    {
        _userProperties.Clear();
        await Task.CompletedTask;
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        _userId = null;
        _userProperties.Clear();
        await Task.CompletedTask;
    }
}
