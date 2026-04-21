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
    private readonly object _lockObject = new();
    private readonly Dictionary<string, string> _userProperties = [];

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

            string? userIdToUse;
            Dictionary<string, string>? userPropsToUse;

            lock (_lockObject)
            {
                userIdToUse = _userId;
                userPropsToUse = _userProperties.Count > 0 ? new Dictionary<string, string>(_userProperties) : null;
            }

            var payload = new TelemetryEventPayload(
                EventName: eventName,
                EventType: "Event",
                UserId: userIdToUse,
                Properties: properties,
                UserProperties: userPropsToUse,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);
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

            string? userIdToUse;
            Dictionary<string, string>? userPropsToUse;

            lock (_lockObject)
            {
                userIdToUse = _userId;
                userPropsToUse = _userProperties.Count > 0 ? new Dictionary<string, string>(_userProperties) : null;
            }

            var pageProperties = new Dictionary<string, object>(properties ?? [])
            {
                { "pageName", pageName }
            };

            var payload = new TelemetryEventPayload(
                EventName: $"PageView_{pageName}",
                EventType: "PageView",
                UserId: userIdToUse,
                Properties: pageProperties,
                UserProperties: userPropsToUse,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);
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

            string? userIdToUse;
            Dictionary<string, string>? userPropsToUse;

            lock (_lockObject)
            {
                userIdToUse = _userId;
                userPropsToUse = _userProperties.Count > 0 ? new Dictionary<string, string>(_userProperties) : null;
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
                UserId: userIdToUse,
                Properties: errorProperties,
                UserProperties: userPropsToUse,
                Platform: _mobileContext.Platform,
                DeviceId: _mobileContext.DeviceId,
                OccurredAt: DateTimeOffset.UtcNow);

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
            await apiClient.PostAsync<object>("/api/telemetry/events", payload, ct);
        }
        catch (Exception trackingError)
        {
            _logger.LogError(trackingError, "Unexpected error sending error telemetry for {ExceptionType}", ex.GetType().Name);
        }
    }

    public void SetUserId(string userId)
    {
        lock (_lockObject)
        {
            _userId = userId;
        }
    }

    public void SetUserProperty(string key, string value)
    {
        lock (_lockObject)
        {
            _userProperties[key] = value;
        }
    }

    public async Task ClearUserPropertiesAsync(CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            _userProperties.Clear();
        }
        await Task.CompletedTask;
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            _userId = null;
            _userProperties.Clear();
        }
        await Task.CompletedTask;
    }
}
