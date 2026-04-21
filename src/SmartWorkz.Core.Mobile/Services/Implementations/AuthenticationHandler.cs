namespace SmartWorkz.Core.Mobile;

using System.Net.Http.Headers;
using SmartWorkz.Core.Shared.Security;

public class AuthenticationHandler : IAuthenticationHandler
{
    private readonly ISecureStorageService _secureStorage;
    private readonly ILogger _logger;
    private readonly JwtSettings _jwtSettings;
    private const string TokenKey = "auth::token";

    public AuthenticationHandler(ISecureStorageService secureStorage, ILogger logger, JwtSettings jwtSettings)
    {
        _secureStorage = Guard.NotNull(secureStorage, nameof(secureStorage));
        _logger = Guard.NotNull(logger, nameof(logger));
        _jwtSettings = Guard.NotNull(jwtSettings, nameof(jwtSettings));
    }

    /// <summary>
    /// Gets the stored authentication token.
    /// </summary>
    public async Task<string?> GetTokenAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var result = await _secureStorage.GetAsync(TokenKey, ct);
            return result.Succeeded ? result.Data : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the authentication token.
    /// </summary>
    public async Task SetTokenAsync(string token, CancellationToken ct = default)
    {
        Guard.NotEmpty(token, nameof(token));
        ct.ThrowIfCancellationRequested();

        try
        {
            await _secureStorage.SetAsync(TokenKey, token, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to set authentication token: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the stored authentication token.
    /// </summary>
    public async Task ClearTokenAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            await _secureStorage.DeleteAsync(TokenKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to clear authentication token: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates if the user is authenticated with a valid token.
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var token = await GetTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var result = JwtHelper.ValidateToken(token, _jwtSettings);
        return result.Succeeded && result.Data?.IsValid == true;
    }

    /// <summary>
    /// Injects authorization headers into an HTTP request.
    /// </summary>
    public async Task InjectHeadersAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        Guard.NotNull(request, nameof(request));
        ct.ThrowIfCancellationRequested();

        var token = await GetTokenAsync(ct);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var result = JwtHelper.ValidateToken(token, _jwtSettings);
            if (result.Succeeded && result.Data?.IsValid == true)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
