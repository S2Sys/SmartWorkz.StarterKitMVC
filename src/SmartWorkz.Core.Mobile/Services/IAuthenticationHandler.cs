namespace SmartWorkz.Mobile;

public interface IAuthenticationHandler
{
    Task<string?> GetTokenAsync(CancellationToken ct = default);
    Task SetTokenAsync(string token, CancellationToken ct = default);
    Task ClearTokenAsync(CancellationToken ct = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);
    Task InjectHeadersAsync(HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Authenticates with email and password, stores access and refresh tokens.</summary>
    Task<Result> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Clears both stored tokens. No server call needed (stateless JWT).</summary>
    Task<Result> LogoutAsync(CancellationToken ct = default);

    /// <summary>Refreshes access token using stored refresh token.</summary>
    Task<Result> RefreshTokenAsync(CancellationToken ct = default);
}
