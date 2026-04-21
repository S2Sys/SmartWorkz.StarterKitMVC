namespace SmartWorkz.Core.Mobile;

#if !WINDOWS
public class MobileService : IMobileService
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;
    private readonly string _sessionDeviceId;
    private readonly IMobileContext _mobileContext;

    public MobileService(IPermissionService permissionService, IMobileContext mobileContext, ILogger logger)
    {
        _permissionService = Guard.NotNull(permissionService, nameof(permissionService));
        _mobileContext = Guard.NotNull(mobileContext, nameof(mobileContext));
        _logger = Guard.NotNull(logger, nameof(logger));
        _sessionDeviceId = Guid.NewGuid().ToString();

        // Populate context once during initialization
        _mobileContext.Platform = GetPlatformInternal();
        _mobileContext.DeviceId = GetDeviceIdInternal();
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
        return GetDeviceIdInternal();
    }

    /// <summary>
    /// Gets the device ID, preferring persistent storage with session-scoped fallback.
    /// </summary>
    private string GetDeviceIdInternal()
    {
        try
        {
            // Check if device ID is already stored in Preferences
            if (Preferences.ContainsKey("device_id"))
            {
                return Preferences.Get("device_id", "");
            }

            // Generate new device ID and store it
            var deviceId = Guid.NewGuid().ToString();
            Preferences.Set("device_id", deviceId);
            return deviceId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to access device preferences; using session ID: {ex.Message}");
            // Fallback to session-scoped GUID if Preferences fails
            // This ensures no user-visible device name is leaked
            return _sessionDeviceId;
        }
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
        return GetPlatformInternal();
    }

    /// <summary>
    /// Gets the platform identifier without logging.
    /// </summary>
    private string GetPlatformInternal()
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
    private readonly string _sessionDeviceId;
    private readonly IMobileContext _mobileContext;

    public MobileService(IPermissionService permissionService, IMobileContext mobileContext, ILogger logger)
    {
        _permissionService = Guard.NotNull(permissionService, nameof(permissionService));
        _mobileContext = Guard.NotNull(mobileContext, nameof(mobileContext));
        _logger = Guard.NotNull(logger, nameof(logger));
        _sessionDeviceId = Guid.NewGuid().ToString();

        // Populate context once during initialization
        _mobileContext.Platform = GetPlatformInternal();
        _mobileContext.DeviceId = GetDeviceIdInternal();
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
        return GetDeviceIdInternal();
    }

    /// <summary>
    /// Gets the device ID, using session-scoped GUID to avoid leaking device names.
    /// </summary>
    private string GetDeviceIdInternal()
    {
        // Use session-scoped GUID instead of Environment.MachineName to avoid
        // leaking user-visible device names in error reports
        return _sessionDeviceId;
    }

    public bool IsTablet()
    {
        return false;
    }

    public string GetPlatform()
    {
        return GetPlatformInternal();
    }

    /// <summary>
    /// Gets the platform identifier.
    /// </summary>
    private string GetPlatformInternal()
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
