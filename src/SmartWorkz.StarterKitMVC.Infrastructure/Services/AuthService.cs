using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.Constants;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService   _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration  _configuration;
    private readonly IEmailQueueRepository _emailQueueRepository;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IEmailQueueRepository emailQueueRepository)
    {
        _userRepository = userRepository;
        _tokenService   = tokenService;
        _passwordHasher = passwordHasher;
        _configuration  = configuration;
        _emailQueueRepository = emailQueueRepository;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Fail<LoginResponse>(MessageKeys.Auth.InvalidCredentials);

        if (!user.IsActive)
            return Result.Fail<LoginResponse>(MessageKeys.Auth.AccountInactive);

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return Result.Fail<LoginResponse>(MessageKeys.Auth.AccountLocked);

        var roles       = await _userRepository.GetUserRolesAsync(user.UserId, request.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, request.TenantId);

        var accessToken  = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            UserId    = user.UserId,
            Token     = refreshToken,
            TenantId  = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        user.AccessFailedCount = 0;
        user.UpdatedAt         = DateTime.UtcNow;
        await _userRepository.UpsertUserAsync(user);

        var expiryMinutes = int.Parse(_configuration["Features:Authentication:Jwt:ExpiryMinutes"] ?? "60");

        return Result.Ok(new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(expiryMinutes),
            MapToProfile(user, roles, permissions)));
    }

    public async Task<Result<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.UserExistsAsync(request.Email, request.TenantId);

        if (exists)
            return Result.Fail<LoginResponse>(MessageKeys.Auth.EmailAlreadyRegistered);

        var user = new User
        {
            UserId            = Guid.NewGuid().ToString(),
            Email             = request.Email,
            NormalizedEmail   = request.Email.ToUpperInvariant(),
            Username          = request.Username,
            NormalizedUsername = request.Username?.ToUpperInvariant(),
            DisplayName       = request.DisplayName ?? request.Username,
            PasswordHash      = _passwordHasher.Hash(request.Password),
            TenantId          = request.TenantId,
            SecurityStamp     = Guid.NewGuid().ToString(),
            ConcurrencyStamp  = Guid.NewGuid().ToString(),
            CreatedAt         = DateTime.UtcNow
        };

        await _userRepository.UpsertUserAsync(user);

        return await LoginAsync(new LoginRequest(request.Email, request.Password, request.TenantId));
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId    = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                     ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var storedToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, null);

        if (storedToken == null)
            return Result.Fail<LoginResponse>(MessageKeys.Auth.InvalidCredentials);

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || !user.IsActive)
            return Result.Fail<LoginResponse>(MessageKeys.Auth.AccountInactive);

        var roles       = await _userRepository.GetUserRolesAsync(user.UserId, user.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, user.TenantId);

        var newAccessToken  = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _userRepository.RevokeRefreshTokenAsync(user.UserId, request.RefreshToken);

        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        await _userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            UserId    = user.UserId,
            Token     = newRefreshToken,
            TenantId  = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        var expiryMinutes = int.Parse(_configuration["Features:Authentication:Jwt:ExpiryMinutes"] ?? "60");

        return Result.Ok(new LoginResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(expiryMinutes),
            MapToProfile(user, roles, permissions)));
    }

    public async Task<Result> RevokeTokenAsync(string userId, string refreshToken)
    {
        await _userRepository.RevokeRefreshTokenAsync(userId, refreshToken);
        return Result.Ok();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        // Always return Ok — don't reveal whether email exists
        if (user == null) return Result.Ok();

        await _userRepository.InvalidatePreviousPasswordResetTokensAsync(user.UserId);

        var token = new PasswordResetToken
        {
            UserId    = user.UserId,
            Token     = _tokenService.GenerateRefreshToken(),
            TenantId  = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreatePasswordResetTokenAsync(token);

        // Queue password reset email with link
        var resetLink = $"{_configuration["App:BaseUrl"]}/account/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token.Token)}";
        var emailQueue = new EmailQueue
        {
            ToEmail = user.Email,
            Subject = "Reset Your Password",
            Body = $"Click the link below to reset your password:\n\n{resetLink}\n\nThis link expires in 2 hours.",
            IsHtml = false,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            SendAttempts = 0,
            TenantId = user.TenantId
        };

        await _emailQueueRepository.EnqueueAsync(emailQueue);
        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null)
            return Result.Fail(MessageKeys.Auth.PasswordResetInvalid);

        var token = await _userRepository.GetPasswordResetTokenAsync(user.UserId, request.Token, request.TenantId);

        if (token == null)
            return Result.Fail(MessageKeys.Auth.PasswordResetInvalid);

        user.PasswordHash  = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt     = DateTime.UtcNow;

        await _userRepository.UpsertUserAsync(user);
        await _userRepository.UsePasswordResetTokenAsync(token.PasswordResetTokenId);

        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return Result.Fail(MessageKeys.Auth.InvalidCredentials);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Fail(MessageKeys.Auth.InvalidCredentials);

        user.PasswordHash  = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt     = DateTime.UtcNow;

        await _userRepository.UpsertUserAsync(user);
        return Result.Ok();
    }

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);

        if (user == null)
            return Result.Fail(MessageKeys.Auth.EmailVerifyInvalid);

        var token = await _userRepository.GetEmailVerificationTokenAsync(user.UserId, request.Token, request.TenantId);

        if (token == null)
            return Result.Fail(MessageKeys.Auth.EmailVerifyInvalid);

        user.EmailConfirmed = true;
        user.UpdatedAt      = DateTime.UtcNow;

        await _userRepository.UpsertUserAsync(user);
        await _userRepository.UseEmailVerificationTokenAsync(token.EmailVerificationTokenId);

        return Result.Ok();
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return Result.Fail<UserProfileDto>(MessageKeys.User.UserNotFound);

        var roles       = await _userRepository.GetUserRolesAsync(user.UserId, user.TenantId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId, user.TenantId);

        return Result.Ok(MapToProfile(user, roles, permissions));
    }

    private static UserProfileDto MapToProfile(User user, List<string> roles, List<string> permissions) => new(
        user.UserId,
        user.Email,
        user.Username,
        user.DisplayName,
        user.AvatarUrl,
        user.TenantId,
        user.EmailConfirmed,
        user.TwoFactorEnabled)
    {
        Roles       = roles,
        Permissions = permissions
    };
}
