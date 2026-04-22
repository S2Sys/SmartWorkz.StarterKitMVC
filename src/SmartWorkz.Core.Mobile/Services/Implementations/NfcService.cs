namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;

/// <summary>
/// Provides NFC (Near Field Communication) services for reading NFC tags and messages.
/// Supports Android and iOS platforms; other platforms gracefully degrade.
/// </summary>
public sealed partial class NfcService : INfcService
{
    private readonly IPermissionService _permissions;
    private readonly ILogger<NfcService> _logger;

    public NfcService(IPermissionService permissions, ILogger<NfcService> logger)
    {
        _permissions = Guard.NotNull(permissions, nameof(permissions));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Reads a message from an NFC tag asynchronously.
    /// Requires NFC hardware availability and user permission.
    /// </summary>
    /// <returns>A Result containing the NFC message if successful, or an error if reading failed, permission denied, or NFC unavailable.</returns>
    public async Task<Result<NfcMessage>> ReadAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var available = await IsAvailableAsync(ct);
        if (!available)
            return Result.Fail<NfcMessage>(new Error("NFC.UNAVAILABLE", "NFC is not available on this device"));

        // Check permission, then request if needed
        var permissionStatus = await _permissions.CheckAsync(MobilePermission.Nfc, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await _permissions.RequestAsync(MobilePermission.Nfc, ct);
        }

        if (permissionStatus != PermissionStatus.Granted)
            return Result.Fail<NfcMessage>(new Error("NFC.PERMISSION_DENIED", "NFC permission denied"));

        try
        {
            var message = await ReadAsyncPlatform(ct);
            return message is not null ? Result.Ok(message) : Result.Fail<NfcMessage>(new Error("NFC.PENDING", "Waiting for NFC tag"));
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<NfcMessage>(new Error("NFC.CANCELLED", "NFC read operation was cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read NFC message");
            return Result.Fail<NfcMessage>(Error.FromException(ex, "NFC.READ_FAILED"));
        }
    }

    /// <summary>
    /// Checks if NFC hardware is available on the device.
    /// </summary>
    /// <returns>True if NFC hardware exists; false if not supported by platform.</returns>
    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        IsAvailableAsyncPlatform(ct);

    /// <summary>
    /// Checks if NFC is enabled by the user in device settings.
    /// </summary>
    /// <returns>True if NFC is enabled; false if disabled or unavailable.</returns>
    public Task<bool> IsEnabledAsync(CancellationToken ct = default) =>
        IsEnabledAsyncPlatform(ct);

    // Platform-specific partial methods
#if __ANDROID__
    private partial Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct);
#elif __IOS__
    private partial Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct);
#else
    private Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct) =>
        Task.FromResult<NfcMessage?>(null);

    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);

    private Task<bool> IsEnabledAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);
#endif
}
