namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

#if !WINDOWS
public class PermissionService : IPermissionService
{
    private readonly ILogger _logger;

    public PermissionService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<PermissionStatus> CheckAsync(MobilePermission permission, CancellationToken ct = default)
    {
        try
        {
            var status = await GetPermissionStatusAsync(permission);
            return MapStatus(status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Permission check failed for {permission}", ex);
            return PermissionStatus.Unknown;
        }
    }

    public async Task<PermissionStatus> RequestAsync(MobilePermission permission, CancellationToken ct = default)
    {
        try
        {
            var status = await RequestPermissionAsync(permission);
            return MapStatus(status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Permission request failed for {permission}", ex);
            return PermissionStatus.Unknown;
        }
    }

    public async Task<Dictionary<MobilePermission, PermissionStatus>> RequestMultipleAsync(CancellationToken ct = default, params MobilePermission[] permissions)
    {
        var results = new Dictionary<MobilePermission, PermissionStatus>();
        foreach (var permission in permissions)
        {
            results[permission] = await RequestAsync(permission, ct);
        }
        return results;
    }

    private async Task<Microsoft.Maui.ApplicationModel.PermissionStatus> GetPermissionStatusAsync(MobilePermission permission)
    {
        return permission switch
        {
            MobilePermission.Camera => await Permissions.CheckStatusAsync<Permissions.Camera>(),
            MobilePermission.Microphone => await Permissions.CheckStatusAsync<Permissions.Microphone>(),
            MobilePermission.Location => await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>(),
            MobilePermission.LocationAlways => await Permissions.CheckStatusAsync<Permissions.LocationAlways>(),
            MobilePermission.Contacts => await Permissions.CheckStatusAsync<Permissions.ContactsRead>(),
            MobilePermission.Gallery => await Permissions.CheckStatusAsync<Permissions.Photos>(),
            MobilePermission.Notifications => await Permissions.CheckStatusAsync<Permissions.PostNotifications>(),
            MobilePermission.Storage => await Permissions.CheckStatusAsync<Permissions.StorageRead>(),
            MobilePermission.Bluetooth => await Permissions.CheckStatusAsync<Permissions.Bluetooth>(),
            MobilePermission.CalendarRead => await Permissions.CheckStatusAsync<Permissions.CalendarRead>(),
            MobilePermission.CalendarWrite => await Permissions.CheckStatusAsync<Permissions.CalendarWrite>(),
            _ => Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown
        };
    }

    private async Task<Microsoft.Maui.ApplicationModel.PermissionStatus> RequestPermissionAsync(MobilePermission permission)
    {
        return permission switch
        {
            MobilePermission.Camera => await Permissions.RequestAsync<Permissions.Camera>(),
            MobilePermission.Microphone => await Permissions.RequestAsync<Permissions.Microphone>(),
            MobilePermission.Location => await Permissions.RequestAsync<Permissions.LocationWhenInUse>(),
            MobilePermission.LocationAlways => await Permissions.RequestAsync<Permissions.LocationAlways>(),
            MobilePermission.Contacts => await Permissions.RequestAsync<Permissions.ContactsRead>(),
            MobilePermission.Gallery => await Permissions.RequestAsync<Permissions.Photos>(),
            MobilePermission.Notifications => await Permissions.RequestAsync<Permissions.PostNotifications>(),
            MobilePermission.Storage => await Permissions.RequestAsync<Permissions.StorageRead>(),
            MobilePermission.Bluetooth => await Permissions.RequestAsync<Permissions.Bluetooth>(),
            MobilePermission.CalendarRead => await Permissions.RequestAsync<Permissions.CalendarRead>(),
            MobilePermission.CalendarWrite => await Permissions.RequestAsync<Permissions.CalendarWrite>(),
            _ => Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown
        };
    }

    private static PermissionStatus MapStatus(Microsoft.Maui.ApplicationModel.PermissionStatus status)
    {
        return status switch
        {
            Microsoft.Maui.ApplicationModel.PermissionStatus.Granted => PermissionStatus.Granted,
            Microsoft.Maui.ApplicationModel.PermissionStatus.Denied => PermissionStatus.Denied,
            Microsoft.Maui.ApplicationModel.PermissionStatus.Disabled => PermissionStatus.DeniedAlways,
            Microsoft.Maui.ApplicationModel.PermissionStatus.Restricted => PermissionStatus.Restricted,
            _ => PermissionStatus.Unknown
        };
    }
}
#else

public class PermissionService : IPermissionService
{
    private readonly ILogger _logger;

    public PermissionService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<PermissionStatus> CheckAsync(MobilePermission permission, CancellationToken ct = default)
    {
        _logger.LogWarning("Permissions not available on Windows platform");
        return PermissionStatus.Granted;
    }

    public async Task<PermissionStatus> RequestAsync(MobilePermission permission, CancellationToken ct = default)
    {
        _logger.LogWarning("Permissions not available on Windows platform");
        return PermissionStatus.Granted;
    }

    public async Task<Dictionary<MobilePermission, PermissionStatus>> RequestMultipleAsync(CancellationToken ct = default, params MobilePermission[] permissions)
    {
        return permissions.ToDictionary(p => p, _ => PermissionStatus.Granted);
    }
}
#endif

