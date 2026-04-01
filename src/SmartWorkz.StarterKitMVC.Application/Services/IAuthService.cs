using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface IAuthService
{
    Task<Result<LoginResponse>>  LoginAsync(LoginRequest request);
    Task<Result<LoginResponse>>  RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>>  RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result>                 RevokeTokenAsync(string userId, string refreshToken);
    Task<Result>                 ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result>                 ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result>                 ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<Result>                 VerifyEmailAsync(VerifyEmailRequest request);
    Task<Result<UserProfileDto>> GetProfileAsync(string userId);
}
