namespace SmartWorkz.Core.Mobile;

public interface IAuthenticationHandler
{
    Task<string?> GetTokenAsync(CancellationToken ct = default);
    Task SetTokenAsync(string token, CancellationToken ct = default);
    Task ClearTokenAsync(CancellationToken ct = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);
    Task InjectHeadersAsync(HttpRequestMessage request, CancellationToken ct = default);
}
