namespace SmartWorkz.Core.Mobile;

/// <summary>
/// Provides stable platform and device context information.
/// This is a lightweight data holder with no service dependencies,
/// designed to break circular dependency patterns between ErrorHandler and MobileService.
/// </summary>
public interface IMobileContext
{
    /// <summary>
    /// Gets or sets the platform identifier (e.g., "iOS", "Android", "Windows").
    /// </summary>
    string Platform { get; set; }

    /// <summary>
    /// Gets or sets the device identifier (persistent GUID or session-scoped fallback).
    /// </summary>
    string DeviceId { get; set; }
}
