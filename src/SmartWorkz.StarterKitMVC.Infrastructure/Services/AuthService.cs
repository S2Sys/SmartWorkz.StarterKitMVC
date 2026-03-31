using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is temporarily locked");

        var roles = await _userRepository.GetUserRolesAsync(user.UserId, request.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, request.TenantId);

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            RefreshTokenId = 0,
            UserId = user.UserId,
            Token = refreshToken,
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        // Reset failed login attempts on success
        user.AccessFailedCount = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        var expiryMinutes = int.Parse(_configuration["Features:Authentication:Jwt:ExpiryMinutes"] ?? "60");

        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(expiryMinutes),
            MapToProfile(user, roles, permissions)
        );
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.UserExistsAsync(request.Email, request.TenantId);

        if (exists)
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            Username = request.Username,
            NormalizedUsername = request.Username?.ToUpperInvariant(),
            DisplayName = request.DisplayName ?? request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            TenantId = request.TenantId,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateUserAsync(user);

        return await LoginAsync(new LoginRequest(request.Email, request.Password, request.TenantId));
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var storedToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, null);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or inactive");

        var roles = await _userRepository.GetUserRolesAsync(user.UserId, user.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, user.TenantId);

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Rotate refresh token
        await _userRepository.RevokeRefreshTokenAsync(user.UserId, request.RefreshToken);

        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            RefreshTokenId = 0,
            UserId = user.UserId,
            Token = newRefreshToken,
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        var expiryMinutes = int.Parse(_configuration["Features:Authentication:Jwt:ExpiryMinutes"] ?? "60");

        return new LoginResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(expiryMinutes),
            MapToProfile(user, roles, permissions)
        );
    }

    public async Task<bool> RevokeTokenAsync(string userId, string refreshToken)
    {
        await _userRepository.RevokeRefreshTokenAsync(userId, refreshToken);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null)
            return true; // Don't reveal whether email exists

        // Invalidate previous tokens
        await _userRepository.InvalidatePreviousPasswordResetTokensAsync(user.UserId);

        var token = new PasswordResetToken
        {
            PasswordResetTokenId = 0,
            UserId = user.UserId,
            Token = _tokenService.GenerateRefreshToken(),
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreatePasswordResetTokenAsync(token);

        // TODO: Send email with reset link containing token.Token
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null)
            return false;

        var token = await _userRepository.GetPasswordResetTokenAsync(user.UserId, request.Token, request.TenantId);

        if (token == null)
            return false;

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        token.UsedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.UpdatePasswordResetTokenAsync(token);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null)
            return false;

        var token = await _userRepository.GetEmailVerificationTokenAsync(user.UserId, request.Token, request.TenantId);

        if (token == null)
            return false;

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        token.VerifiedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.UpdateEmailVerificationTokenAsync(token);
        return true;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(user.UserId, user.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, user.TenantId);

        return MapToProfile(user, roles, permissions);
    }

    private static UserProfileDto MapToProfile(User user, List<string> roles, List<string> permissions) => new(
        user.UserId,
        user.Email,
        user.Username,
        user.DisplayName,
        user.AvatarUrl,
        user.TenantId,
        user.EmailConfirmed,
        user.TwoFactorEnabled
    )
    {
        Roles = roles,
        Permissions = permissions
    };
}
