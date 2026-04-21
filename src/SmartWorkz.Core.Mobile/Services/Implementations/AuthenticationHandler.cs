namespace SmartWorkz.Core.Mobile;

using System.Net.Http.Headers;

public class AuthenticationHandler : IAuthenticationHandler
{
    private readonly ISecureStorageService _secureStorage;
    private readonly ILogger _logger;
    private const string TokenKey = "auth::token";

    public AuthenticationHandler(ISecureStorageService secureStorage, ILogger logger)
    {
        _secureStorage = Guard.NotNull(secureStorage, nameof(secureStorage));
        _logger = Guard.NotNull(logger, nameof(logger));
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

        return ValidateToken(token);
    }

    /// <summary>
    /// Injects authorization headers into an HTTP request.
    /// </summary>
    public async Task InjectHeadersAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        Guard.NotNull(request, nameof(request));
        ct.ThrowIfCancellationRequested();

        var token = await GetTokenAsync(ct);

        if (!string.IsNullOrWhiteSpace(token) && ValidateToken(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Validates a JWT token (basic validation).
    /// </summary>
    private bool ValidateToken(string token)
    {
        try
        {
            // Basic JWT validation: check format (3 parts separated by dots)
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid token format");
                return false;
            }

            // Decode payload to check expiration
            var payload = parts[1];
            // Add padding if needed
            payload += new string('=', (4 - payload.Length % 4) % 4);

            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var exp))
            {
                var expirationTime = UnixTimeStampToDateTime(exp);
                if (expirationTime < DateTime.UtcNow)
                {
                    _logger.LogDebug("Token has expired");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Token validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimestamp).ToUniversalTime();
        return dateTime;
    }
}
