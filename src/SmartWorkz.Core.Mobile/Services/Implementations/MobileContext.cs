namespace SmartWorkz.Mobile;

/// <summary>
/// Provides mutable platform and device context information.
/// This is a simple data holder designed to break circular dependency patterns.
/// </summary>
public class MobileContext : IMobileContext
{
    private string _platform = "";
    private string _deviceId = "";

    /// <summary>
    /// Gets or sets the platform identifier.
    /// </summary>
    public string Platform
    {
        get => _platform;
        set => _platform = value;
    }

    /// <summary>
    /// Gets or sets the device identifier.
    /// </summary>
    public string DeviceId
    {
        get => _deviceId;
        set => _deviceId = value;
    }
}
