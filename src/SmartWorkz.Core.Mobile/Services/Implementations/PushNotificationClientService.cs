namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.Logging;

/// <summary>Cross-platform push notification registration service.</summary>
internal partial class PushNotificationClientService : IPushNotificationClientService
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorageService;
    private readonly IMobileContext _mobileContext;
    private readonly ILogger _logger;

    private const string PushTokenKey = "fcm::device_token";

    public PushNotificationClientService(
        IApiClient apiClient,
        ISecureStorageService secureStorageService,
        IMobileContext mobileContext,
        ILogger logger)
    {
        _apiClient = Guard.NotNull(apiClient, nameof(apiClient));
        _secureStorageService = Guard.NotNull(secureStorageService, nameof(secureStorageService));
        _mobileContext = Guard.NotNull(mobileContext, nameof(mobileContext));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Result> RegisterAsync(CancellationToken ct = default)
    {
        try
        {
            var token = await GetPushTokenAsync(ct);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Failed to get push notification token");
                return Result.Fail(Error.Invalid("Push token unavailable"));
            }

            // Store token locally
            var storeResult = await _secureStorageService.SetAsync(PushTokenKey, token, ct);
            if (!storeResult.Succeeded)
            {
                _logger.LogWarning("Failed to store push token locally");
                return storeResult;
            }

            // Register with backend
            var payload = new { DeviceToken = token, Platform = _mobileContext.Platform };
            var result = await _apiClient.PostAsync<object>("/api/notifications/register", payload, ct);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to register push token with backend: {ErrorCode}", result.Error?.Code ?? "UNKNOWN");
                return result.AsError();
            }

            _logger.LogInformation("Push notifications registered");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error registering push notifications");
            return Result.Fail(Error.Unexpected("Push notification registration failed"));
        }
    }

    public async Task<Result> UnregisterAsync(CancellationToken ct = default)
    {
        try
        {
            var tokenResult = await _secureStorageService.GetAsync(PushTokenKey, ct);
            var token = tokenResult.Succeeded ? tokenResult.Data : null;

            if (!string.IsNullOrEmpty(token))
            {
                // Unregister from backend (idempotent - ignore failures)
                var result = await _apiClient.DeleteAsync($"/api/notifications/unregister?deviceToken={Uri.EscapeDataString(token)}", ct);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to unregister push token with backend: {ErrorCode}", result.Error?.Code ?? "UNKNOWN");
                }
            }

            // Clear local token
            await _secureStorageService.DeleteAsync(PushTokenKey, ct);

            _logger.LogInformation("Push notifications unregistered");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error unregistering push notifications");
            return Result.Fail(Error.Unexpected("Push notification unregistration failed"));
        }
    }

    public async Task<string?> GetPushTokenAsync(CancellationToken ct = default)
    {
        try
        {
            return await GetPushTokenAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get push token");
            return null;
        }
    }

    // Platform-specific partial method
    private partial Task<string?> GetPushTokenAsyncPlatform(CancellationToken ct);
}
