namespace SmartWorkz.Mobile;

/// <summary>
/// Service for reading NFC (Near Field Communication) messages from compatible tags and devices.
/// Supported on Android and iOS (iOS 13.1+).
/// </summary>
public interface INfcService
{
    /// <summary>
    /// Reads an NFC message from a nearby tag or device asynchronously.
    /// Blocks until an NFC tag is detected or the operation is cancelled.
    /// Requires NFC permission and NFC to be enabled on the device.
    /// Throws OperationCanceledException if the CancellationToken is signaled.
    /// Returns a Result containing the parsed NFC message with type, payload, and detection timestamp.
    /// On unsupported platforms or when NFC is disabled, returns an appropriate error result.
    /// </summary>
    Task<Result<NfcMessage>> ReadAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if NFC hardware is available on this device.
    /// This indicates the device has NFC hardware, regardless of whether it's currently enabled.
    /// Returns true if NFC hardware exists, false otherwise.
    /// Difference from IsEnabledAsync: IsAvailableAsync only checks hardware presence, while IsEnabledAsync checks if NFC is active.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if NFC is currently enabled and active on this device.
    /// This indicates the device user has enabled NFC in system settings.
    /// Returns true if NFC is enabled, false if disabled or unavailable.
    /// Difference from IsAvailableAsync: IsEnabledAsync checks the current state, while IsAvailableAsync only checks hardware presence.
    /// </summary>
    Task<bool> IsEnabledAsync(CancellationToken ct = default);
}
