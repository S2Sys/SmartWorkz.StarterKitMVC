namespace SmartWorkz.Core.Mobile;

public interface IConnectionChecker
{
    Task<bool> IsOnlineAsync(CancellationToken ct = default);
    NetworkType GetNetworkType();
    IObservable<bool> OnConnectivityChanged();
}
