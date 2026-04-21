namespace SmartWorkz.Core.Mobile;

using System.Net.Http.Headers;
using SmartWorkz.Core.Shared.Security;

public class AuthenticationHandler : IAuthenticationHandler
{
    private readonly ISecureStorageService _secureStorage;
    private readonly ILogger _logger;
    private readonly JwtSettings _jwtSettings;
    private readonly IMobileContext _mobileContext;
    private readonly Lazy<IApiClient> _apiClient;
    private const string TokenKey = "auth::token";
    private const string RefreshTokenKey = "auth::refresh_token";

    public AuthenticationHandler(
        ISecureStorageService secureStorage,
        ILogger logger,
        JwtSettings jwtSettings,
        IMobileContext mobileContext,
        Lazy<IApiClient> apiClient)
    {
        _secureStorage = Guard.NotNull(secureStorage, nameof(secureStorage));
        _logger = Guard.NotNull(logger, nameof(logger));
        _jwtSettings = Guard.NotNull(jwtSettings, nameof(jwtSettings));
        _mobileContext = Guard.NotNull(mobileContext, nameof(mobileContext));
        _apiClient = Guard.NotNull(apiClient, nameof(apiClient));
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get authentication token", ex);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to set authentication token", ex);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to clear authentication token", ex);
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

    /// <summary>
    /// Authenticates with email and password, stores access and refresh tokens.
    /// </summary>
    public async Task<Result> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(email))
            return Result.Fail(Error.Validation("email", "Email is required"));
        if (string.IsNullOrEmpty(password))
            return Result.Fail(Error.Validation("password", "Password is required"));

        var request = new LoginRequest(email, password);
        var result = await _apiClient.Value.PostAsync<AuthTokenResponse>("/api/auth/login", request, ct);

        if (!result.Succeeded)
            return Result.Fail(result.Error ?? new Error("AUTH.LOGIN_FAILED", "Login failed"));

        // Store both tokens
        await _secureStorage.SetAsync(TokenKey, result.Data!.AccessToken, ct);
        await _secureStorage.SetAsync(RefreshTokenKey, result.Data.RefreshToken, ct);

        _logger.LogInformation("User {Email} logged in successfully", email);
        return Result.Ok();
    }

    /// <summary>
    /// Clears both stored tokens. No server call needed (stateless JWT).
    /// </summary>
    public async Task<Result> LogoutAsync(CancellationToken ct = default)
    {
        // Clear tokens from secure storage
        await _secureStorage.DeleteAsync(TokenKey, ct);
        await _secureStorage.DeleteAsync(RefreshTokenKey, ct);

        _logger.LogInformation("User logged out successfully");
        return Result.Ok();
    }

    /// <summary>
    /// Refreshes access token using stored refresh token.
    /// </summary>
    public async Task<Result> RefreshTokenAsync(CancellationToken ct = default)
    {
        var tokenResult = await _secureStorage.GetAsync(RefreshTokenKey, ct);
        if (!tokenResult.Succeeded || string.IsNullOrEmpty(tokenResult.Data))
            return Result.Fail(Error.Unauthorized("No refresh token available"));

        var request = new RefreshRequest(tokenResult.Data);
        var result = await _apiClient.Value.PostAsync<AuthTokenResponse>("/api/auth/refresh", request, ct);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Token refresh failed: {ErrorCode} - {ErrorMessage}",
                result.Error?.Code ?? "UNKNOWN", result.Error?.Message ?? "Unknown error");
            // Clear invalid tokens
            await LogoutAsync(ct);
            return Result.Fail(result.Error ?? new Error("AUTH.REFRESH_FAILED", "Token refresh failed"));
        }

        // Update tokens
        await _secureStorage.SetAsync(TokenKey, result.Data!.AccessToken, ct);
        await _secureStorage.SetAsync(RefreshTokenKey, result.Data.RefreshToken, ct);

        _logger.LogDebug("Tokens refreshed successfully");
        return Result.Ok();
    }
}
