namespace SmartWorkz.Core.Mobile;

public interface IMobileService
{
    DeviceType GetDeviceType();
    ScreenOrientation GetOrientation();
    string GetDeviceId();
    bool IsTablet();
    string GetPlatform();
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);
    AppTheme GetCurrentTheme();
}
