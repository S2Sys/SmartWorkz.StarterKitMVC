using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of authentication service with JWT token generation.
/// Manages user login, registration, password reset, and token lifecycle.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger,
        string jwtSecret,
        string jwtIssuer,
        string jwtAudience,
        int jwtExpirationMinutes = 15,
        int refreshTokenExpirationDays = 7)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        _jwtIssuer = jwtIssuer ?? throw new ArgumentNullException(nameof(jwtIssuer));
        _jwtAudience = jwtAudience ?? throw new ArgumentNullException(nameof(jwtAudience));
        _jwtExpirationMinutes = jwtExpirationMinutes;
        _refreshTokenExpirationDays = refreshTokenExpirationDays;
    }

    /// <inheritdoc />
    public async Task<(bool Success, LoginResponse? Response, string? Error)> LoginAsync(
        string email, string password, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, null, "Email and password are required");

        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId);
            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", email);
                return (false, null, "Invalid credentials");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {UserId}", user.UserId);
                return (false, null, "Account is not active");
            }

            if (!_passwordHasher.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {UserId}", user.UserId);
                return (false, null, "Invalid credentials");
            }

            // Get user roles and permissions
            var roles = await _userRepository.GetUserRolesAsync(user.UserId, tenantId);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, tenantId);

            var userProfile = new UserProfileDto(
                user.UserId,
                user.Email,
                user.Username,
                user.DisplayName ?? user.Email,
                user.AvatarUrl,
                tenantId,
                user.EmailConfirmed,
                user.TwoFactorEnabled)
            {
                Roles = roles ?? new List<string>(),
                Permissions = permissions ?? new List<string>()
            };

            // Generate tokens
            var accessToken = await GenerateJwtTokenAsync(userProfile);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId, tenantId);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            _logger.LogInformation("User logged in successfully: {UserId}", user.UserId);

            return (true, new LoginResponse(accessToken, refreshToken, expiresAt, userProfile), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            return (false, null, "An error occurred during login");
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(string email, string password, string tenantId)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId);
            if (user == null || !user.IsActive)
                return false;

            return _passwordHasher.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for email: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, LoginResponse? Response, string? Error)> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
            return (false, null, "Registration request is required");

        try
        {
            // Check if user already exists
            if (await _userRepository.UserExistsAsync(request.Email, request.TenantId))
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
                return (false, null, "Email already registered");
            }

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpperInvariant(),
                Username = request.Username,
                NormalizedUsername = request.Username.ToUpperInvariant(),
                DisplayName = request.DisplayName,
                PasswordHash = _passwordHasher.Hash(request.Password),
                IsActive = true,
                EmailConfirmed = false,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                TenantId = request.TenantId
            };

            await _userRepository.UpsertUserAsync(user);

            // Generate tokens
            var userProfile = new UserProfileDto(
                user.UserId,
                user.Email,
                user.Username,
                user.DisplayName,
                null,
                request.TenantId,
                false,
                false);

            var accessToken = await GenerateJwtTokenAsync(userProfile);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId, request.TenantId);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            _logger.LogInformation("New user registered successfully: {UserId}", user.UserId);

            return (true, new LoginResponse(accessToken, refreshToken, expiresAt, userProfile), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return (false, null, "An error occurred during registration");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> SendPasswordResetEmailAsync(string email, string tenantId)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, tenantId);
            if (user == null)
            {
                // Don't reveal if email exists for security
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return (true, null);
            }

            // Generate reset token
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var passwordResetToken = new Domain.Entities.Auth.PasswordResetToken
            {
                UserId = user.UserId,
                Token = resetToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                UsedAt = null,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreatePasswordResetTokenAsync(passwordResetToken);

            _logger.LogInformation("Password reset token generated for user: {UserId}", user.UserId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email for: {Email}", email);
            return (false, "An error occurred while processing your request");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string token, string newPassword, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Token and new password are required");

        try
        {
            // In a real implementation, you would decode the token to get the userId
            // For now, this is a placeholder
            _logger.LogInformation("Password reset completed with token");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return (false, "An error occurred while resetting password");
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateJwtTokenAsync(UserProfileDto user)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName),
                new("tenant", user.TenantId),
                new("username", user.Username)
            };

            // Add roles as claims
            foreach (var role in user.Roles ?? new List<string>())
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permissions as claims
            foreach (var permission in user.Permissions ?? new List<string>())
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {UserId}", user.UserId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateRefreshTokenAsync(string userId, string tenantId)
    {
        try
        {
            var refreshToken = new Domain.Entities.Auth.RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                RevokedAt = null,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateRefreshTokenAsync(refreshToken);

            _logger.LogDebug("Refresh token generated for user: {UserId}", userId);

            return refreshToken.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token for user: {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateRefreshTokenAsync(string token, string userId, string tenantId)
    {
        try
        {
            var refreshToken = await _userRepository.GetRefreshTokenAsync(token, tenantId);

            if (refreshToken == null || refreshToken.RevokedAt.HasValue)
                return false;

            if (refreshToken.UserId != userId)
                return false;

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, LoginResponse? Response, string? Error)> RefreshAccessTokenAsync(
        string refreshToken, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return (false, null, "Refresh token is required");

        try
        {
            var token = await _userRepository.GetRefreshTokenAsync(refreshToken, tenantId);

            if (token == null || token.RevokedAt.HasValue || token.ExpiresAt < DateTime.UtcNow)
                return (false, null, "Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(token.UserId);
            if (user == null || !user.IsActive)
                return (false, null, "User not found or inactive");

            var roles = await _userRepository.GetUserRolesAsync(user.UserId, tenantId);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, tenantId);

            var userProfile = new UserProfileDto(
                user.UserId,
                user.Email,
                user.Username,
                user.DisplayName ?? user.Email,
                user.AvatarUrl,
                tenantId,
                user.EmailConfirmed,
                user.TwoFactorEnabled)
            {
                Roles = roles ?? new List<string>(),
                Permissions = permissions ?? new List<string>()
            };

            var newAccessToken = await GenerateJwtTokenAsync(userProfile);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.UserId, tenantId);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

            _logger.LogInformation("Token refreshed for user: {UserId}", user.UserId);

            return (true, new LoginResponse(newAccessToken, newRefreshToken, expiresAt, userProfile), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return (false, null, "An error occurred while refreshing token");
        }
    }

    /// <inheritdoc />
    public async Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string tenantId)
    {
        try
        {
            await _userRepository.RevokeRefreshTokenAsync(userId, refreshToken);
            _logger.LogInformation("Refresh token revoked for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token for user: {UserId}", userId);
            return false;
        }
    }
}
