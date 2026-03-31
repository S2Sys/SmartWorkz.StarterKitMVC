using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AuthDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.TenantId == request.TenantId &&
                !u.IsDeleted);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is temporarily locked");

        var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new();
        var permissions = user.UserPermissions?.Select(up => up.Permission.Name).ToList() ?? new();

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        // Reset failed login attempts on success
        user.AccessFailedCount = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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
        var exists = await _context.Users.AnyAsync(u =>
            u.Email == request.Email &&
            u.TenantId == request.TenantId &&
            !u.IsDeleted);

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

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await LoginAsync(new LoginRequest(request.Email, request.Password, request.TenantId));
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.UserId == userId &&
                rt.Token == request.RefreshToken &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow &&
                !rt.IsDeleted);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or inactive");

        var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new();
        var permissions = user.UserPermissions?.Select(up => up.Permission.Name).ToList() ?? new();

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Rotate refresh token
        storedToken.RevokedAt = DateTime.UtcNow;

        var expiryDays = int.Parse(_configuration["Features:Authentication:Jwt:RefreshTokenExpiryDays"] ?? "7");
        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            Token = newRefreshToken,
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

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
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken && rt.RevokedAt == null);

        if (token == null)
            return false;

        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.TenantId == request.TenantId &&
                !u.IsDeleted);

        if (user == null)
            return true; // Don't reveal whether email exists

        // Invalidate previous tokens
        var existing = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.UsedAt == null && !t.IsDeleted)
            .ToListAsync();
        existing.ForEach(t => t.IsDeleted = true);

        var token = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = _tokenService.GenerateRefreshToken(), // reuse secure random gen
            TenantId = user.TenantId,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        // TODO: Send email with reset link containing token.Token
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.TenantId == request.TenantId &&
                !u.IsDeleted);

        if (user == null)
            return false;

        var token = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t =>
                t.UserId == user.UserId &&
                t.Token == request.Token &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTime.UtcNow &&
                !t.IsDeleted);

        if (token == null)
            return false;

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        token.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.TenantId == request.TenantId &&
                !u.IsDeleted);

        if (user == null)
            return false;

        var token = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t =>
                t.UserId == user.UserId &&
                t.Token == request.Token &&
                t.VerifiedAt == null &&
                t.ExpiresAt > DateTime.UtcNow &&
                !t.IsDeleted);

        if (token == null)
            return false;

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        token.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserPermissions).ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null)
            return null;

        var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new();
        var permissions = user.UserPermissions?.Select(up => up.Permission.Name).ToList() ?? new();

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
