using SmartWorkz.Extensions;
using Microsoft.Extensions.Logging;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Authentication API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • POST /api/authentication/login - User login
/// • POST /api/authentication/register - User registration
/// • POST /api/authentication/refresh - Refresh JWT token
/// • POST /api/authentication/logout - User logout
/// • POST /api/authentication/change-password - Change user password
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // Login and get JWT token
/// var response = await client.Authentication.LoginAsync(
///     new LoginRequest("user@example.com", "password123"));
/// var token = response.Token;
///
/// // Register new user
/// var registerResponse = await client.Authentication.RegisterAsync(
///     new RegisterRequest("newuser@example.com", "John", "Doe", "password123"));
///
/// // Refresh token
/// var refreshResponse = await client.Authentication.RefreshTokenAsync(
///     new RefreshTokenRequest(currentRefreshToken));
/// </code>
/// </para>
/// </summary>
public interface IAuthenticationClient
{
    /// <summary>
    /// Authenticates a user with email and password, returning a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response with JWT token.</returns>
    Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response.</returns>
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired JWT token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New authentication response with updated token.</returns>
    Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">Password change request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response indicating success or failure.</returns>
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out the current user (invalidates token on server).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response indicating logout status.</returns>
    Task<ApiResponse<string>> LogoutAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IAuthenticationClient"/>.
/// </summary>
public class AuthenticationClient : IAuthenticationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationClient>? _logger;

    public AuthenticationClient(HttpClient httpClient, ILogger<AuthenticationClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Authenticating user: {Email}", request.Email);

        var response = await _httpClient.PostAsJsonAsync("/api/authentication/login", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<AuthenticationResponse>(cancellationToken);

        if (result?.Success == true)
            _logger?.LogInformation("User authenticated successfully: {Email}", request.Email);
        else
            _logger?.LogWarning("Authentication failed for user: {Email}", request.Email);

        return result;
    }

    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Registering new user: {Email}", request.Email);

        var response = await _httpClient.PostAsJsonAsync("/api/authentication/register", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<AuthenticationResponse>(cancellationToken);

        if (result?.Success == true)
            _logger?.LogInformation("User registered successfully: {Email}", request.Email);
        else
            _logger?.LogWarning("Registration failed for user: {Email}", request.Email);

        return result;
    }

    public async Task<AuthenticationResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogDebug("Refreshing authentication token");

        var response = await _httpClient.PostAsJsonAsync("/api/authentication/refresh", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<AuthenticationResponse>(cancellationToken);

        if (result?.Success == true)
            _logger?.LogDebug("Token refreshed successfully");
        else
            _logger?.LogWarning("Token refresh failed");

        return result;
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Changing user password");

        var response = await _httpClient.PostAsJsonAsync("/api/authentication/change-password", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<ApiResponse<string>>(cancellationToken);

        if (result?.Success == true)
            _logger?.LogInformation("Password changed successfully");
        else
            _logger?.LogWarning("Password change failed");

        return result;
    }

    public async Task<ApiResponse<string>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Logging out user");

        var response = await _httpClient.PostAsync("/api/authentication/logout", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<ApiResponse<string>>(cancellationToken);

        if (result?.Success == true)
            _logger?.LogInformation("User logged out successfully");

        return result;
    }
}
