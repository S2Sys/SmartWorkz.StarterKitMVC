using SmartWorkz.Mobile.Models;

namespace SmartWorkz.Mobile.Services;

/// <summary>
/// Cross-platform helper for managing location permissions.
/// Provides unified API for requesting and checking permissions across iOS, Android, Windows, and macOS.
/// </summary>
public static class LocationPermissionHelper
{
    /// <summary>
    /// Maps platform-specific permission status to our unified enum.
    /// </summary>
    /// <param name="permissionStatus">The platform-specific permission status.</param>
    /// <returns>Unified PermissionStatus value.</returns>
    /// <remarks>
    /// Currently a pass-through operation as PermissionStatus is already unified across all platforms (iOS, Android, Windows, macOS).
    /// The PermissionStatus enum (NotRequested, Denied, WhenInUse, Always) directly matches platform conventions,
    /// eliminating the need for translation. This method is kept for API consistency and future extensibility if
    /// platform-specific normalization becomes necessary.
    /// </remarks>
    public static PermissionStatus NormalizePermissionStatus(PermissionStatus permissionStatus)
    {
        return permissionStatus;
    }

    /// <summary>
    /// Checks if location service is available on this platform.
    /// </summary>
    /// <returns>True if location services are available, false otherwise.</returns>
    public static bool IsLocationServiceAvailable()
    {
        return OperatingSystem.IsIOS() ||
               OperatingSystem.IsAndroid() ||
               OperatingSystem.IsWindows() ||
               OperatingSystem.IsMacOS();
    }

    /// <summary>
    /// Gets the appropriate permission description for the current platform.
    /// </summary>
    /// <returns>User-facing permission description.</returns>
    public static string GetPermissionDescription()
    {
        return OperatingSystem.IsIOS() || OperatingSystem.IsMacOS()
            ? "This app needs access to your location to provide location services."
            : OperatingSystem.IsAndroid()
            ? "This app needs permission to access your device location."
            : "This app needs access to your device's location services.";
    }

    /// <summary>
    /// Determines if a permission status indicates access is granted.
    /// </summary>
    public static bool IsPermissionGranted(PermissionStatus status)
    {
        return status == PermissionStatus.WhenInUse || status == PermissionStatus.Always;
    }

    /// <summary>
    /// Determines if a permission status indicates access was denied.
    /// </summary>
    public static bool IsPermissionDenied(PermissionStatus status)
    {
        return status == PermissionStatus.Denied;
    }

    /// <summary>
    /// Determines if a permission status indicates permission has never been requested.
    /// </summary>
    public static bool IsPermissionNotRequested(PermissionStatus status)
    {
        return status == PermissionStatus.NotRequested;
    }
}
