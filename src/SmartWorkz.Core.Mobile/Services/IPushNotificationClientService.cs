namespace SmartWorkz.Core.Mobile;

public interface IPushNotificationClientService
{
    /// <summary>Registers the device for push notifications.</summary>
    Task<Result> RegisterAsync(CancellationToken ct = default);

    /// <summary>Unregisters the device from push notifications.</summary>
    Task<Result> UnregisterAsync(CancellationToken ct = default);

    /// <summary>Gets the current push notification device token.</summary>
    Task<string?> GetPushTokenAsync(CancellationToken ct = default);
}
