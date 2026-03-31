using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Shared.Validation;

/// <summary>
/// Validation rules for authentication DTOs.
/// </summary>
public static class AuthValidators
{
    public static bool IsValidLogin(LoginRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.Password)
            && !string.IsNullOrWhiteSpace(request.TenantId);
    }

    public static bool IsValidRegister(RegisterRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.Password)
            && !string.IsNullOrWhiteSpace(request.TenantId)
            && request.Password.Length >= 8
            && IsValidEmail(request.Email);
    }

    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPassword(string password)
    {
        // At least 8 chars, 1 uppercase, 1 lowercase, 1 digit
        return password.Length >= 8
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit);
    }

    public static bool IsValidRefreshToken(RefreshTokenRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.AccessToken)
            && !string.IsNullOrWhiteSpace(request.RefreshToken);
    }

    public static bool IsValidForgotPassword(ForgotPasswordRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.TenantId)
            && IsValidEmail(request.Email);
    }

    public static bool IsValidResetPassword(ResetPasswordRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.Token)
            && !string.IsNullOrWhiteSpace(request.NewPassword)
            && !string.IsNullOrWhiteSpace(request.TenantId)
            && IsValidPassword(request.NewPassword);
    }

    public static bool IsValidChangePassword(ChangePasswordRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.CurrentPassword)
            && !string.IsNullOrWhiteSpace(request.NewPassword)
            && request.CurrentPassword != request.NewPassword
            && IsValidPassword(request.NewPassword);
    }

    public static bool IsValidVerifyEmail(VerifyEmailRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.Token)
            && !string.IsNullOrWhiteSpace(request.TenantId)
            && IsValidEmail(request.Email);
    }
}
