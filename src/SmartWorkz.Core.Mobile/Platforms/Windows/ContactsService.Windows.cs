namespace SmartWorkz.Mobile;

#if WINDOWS

/// <summary>
/// Windows platform stub for the contacts service.
/// Contact access is not currently implemented for Windows desktop applications.
/// </summary>
/// <remarks>
/// Windows desktop does not provide a standardized way to access system contacts through MAUI.
/// Users running on Windows desktop should use platform-specific solutions or cloud-based contact APIs.
/// This stub exists to allow cross-platform compilation but provides no functionality.
/// </remarks>
public partial class ContactsService
{
    // Windows implementation - contacts not available on Windows platform
    // The non-Windows partial methods are not needed for Windows builds
}

#endif
