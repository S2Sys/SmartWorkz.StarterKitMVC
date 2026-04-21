namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

#if !WINDOWS
using System.Reactive.Linq;

public class ConnectionChecker : IConnectionChecker
{
    private readonly ILogger _logger;

    public ConnectionChecker(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<bool> IsOnlineAsync(CancellationToken ct = default)
    {
        var access = Connectivity.Current.NetworkAccess;
        return access == NetworkAccess.Internet;
    }

    public NetworkType GetNetworkType()
    {
        var profiles = Connectivity.Current.ConnectionProfiles;

        if (!profiles.Any())
            return NetworkType.None;

        return profiles.First() switch
        {
            ConnectionProfile.WiFi => NetworkType.WiFi,
            ConnectionProfile.Cellular => NetworkType.Cellular,
            ConnectionProfile.Ethernet => NetworkType.Ethernet,
            _ => NetworkType.Unknown
        };
    }

    public IObservable<bool> OnConnectivityChanged()
    {
        return Observable.FromEvent<EventHandler<ConnectivityChangedEventArgs>, bool>(
            handler =>
            {
                void ConnectivityChanged(object? sender, ConnectivityChangedEventArgs args) =>
                    handler(args.NetworkAccess == NetworkAccess.Internet);

                return ConnectivityChanged;
            },
            handler => Connectivity.ConnectivityChanged += handler,
            handler => Connectivity.ConnectivityChanged -= handler
        );
    }
}
#else

public class ConnectionChecker : IConnectionChecker
{
    private readonly ILogger _logger;

    public ConnectionChecker(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<bool> IsOnlineAsync(CancellationToken ct = default)
    {
        return false;
    }

    public NetworkType GetNetworkType()
    {
        return NetworkType.Unknown;
    }

    public IObservable<bool> OnConnectivityChanged()
    {
        throw new PlatformNotSupportedException("Connectivity not available on Windows platform");
    }
}
#endif

