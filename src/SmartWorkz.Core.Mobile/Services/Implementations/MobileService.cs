namespace SmartWorkz.Core.Mobile;

#if !WINDOWS
public class MobileService : IMobileService
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;

    public MobileService(IPermissionService permissionService, ILogger logger)
    {
        _permissionService = Guard.NotNull(permissionService, nameof(permissionService));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Gets the device type based on MAUI DeviceInfo.
    /// </summary>
    public DeviceType GetDeviceType()
    {
        var idiom = DeviceInfo.Current.Idiom;
        if (idiom == DeviceIdiom.Phone)
            return DeviceType.Phone;
        if (idiom == DeviceIdiom.Tablet)
            return DeviceType.Tablet;
        if (idiom == DeviceIdiom.Desktop)
            return DeviceType.Desktop;
        if (idiom == DeviceIdiom.Watch)
            return DeviceType.Watch;

        return DeviceType.Unknown;
    }

    /// <summary>
    /// Gets the current screen orientation.
    /// </summary>
    public ScreenOrientation GetOrientation()
    {
        try
        {
            var rotation = DeviceDisplay.Current.MainDisplayInfo.Rotation;
            if (rotation == DisplayRotation.Rotation0 || rotation == DisplayRotation.Rotation180)
                return ScreenOrientation.Portrait;
            if (rotation == DisplayRotation.Rotation90 || rotation == DisplayRotation.Rotation270)
                return ScreenOrientation.Landscape;
        }
        catch
        {
            // Fallback if display info not available
        }

        return ScreenOrientation.Unknown;
    }

    /// <summary>
    /// Gets the unique device identifier.
    /// </summary>
    public string GetDeviceId()
    {
        return DeviceInfo.Current.Name ?? "unknown";
    }

    /// <summary>
    /// Determines if the device is a tablet.
    /// </summary>
    public bool IsTablet()
    {
        return DeviceInfo.Current.Idiom == DeviceIdiom.Tablet;
    }

    /// <summary>
    /// Gets the platform name (iOS, Android, macOS, Windows).
    /// </summary>
    public string GetPlatform()
    {
        #if __IOS__
        return "iOS";
        #elif __ANDROID__
        return "Android";
        #elif __MACCATALYST__
        return "macOS";
        #else
        return "Unknown";
        #endif
    }

    /// <summary>
    /// Checks if the app has the specified permission.
    /// </summary>
    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        Guard.NotEmpty(permission, nameof(permission));

        if (!Enum.TryParse<MobilePermission>(permission, ignoreCase: true, out var mobilePermission))
        {
            _logger.LogWarning($"Invalid permission string: {permission}");
            return false;
        }

        var status = await _permissionService.CheckAsync(mobilePermission, ct);
        return status == PermissionStatus.Granted;
    }

    /// <summary>
    /// Gets the current app theme setting.
    /// </summary>
    public AppTheme GetCurrentTheme()
    {
        try
        {
            // Theme detection is platform-specific and MAUI abstracts it differently
            // For a complete implementation, this would need platform-specific code
            // For now, return System as the safe default
            return AppTheme.System;
        }
        catch
        {
            return AppTheme.System;
        }
    }
}
#else

public class MobileService : IMobileService
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;

    public MobileService(IPermissionService permissionService, ILogger logger)
    {
        _permissionService = Guard.NotNull(permissionService, nameof(permissionService));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public DeviceType GetDeviceType()
    {
        return DeviceType.Desktop;
    }

    public ScreenOrientation GetOrientation()
    {
        return ScreenOrientation.Landscape;
    }

    public string GetDeviceId()
    {
        return Environment.MachineName;
    }

    public bool IsTablet()
    {
        return false;
    }

    public string GetPlatform()
    {
        return "Windows";
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        _logger.LogWarning("Permissions not available on Windows platform");
        return true;
    }

    public AppTheme GetCurrentTheme()
    {
        return AppTheme.System;
    }
}
#endif
