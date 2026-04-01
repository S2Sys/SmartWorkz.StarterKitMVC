using Microsoft.AspNetCore.Authorization;
using SmartWorkz.StarterKitMVC.Shared.Constants;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Demo;

/// <summary>
/// Demo page showing all translation keys and their current values.
/// Demonstrates the translation system for developers.
/// </summary>
[AllowAnonymous]
public class TranslationsModel : BasePage
{
    public List<TranslationEntry> Entries { get; private set; } = [];

    public void OnGet()
    {
        // Build list of all MessageKeys with their translated values
        Entries = new()
        {
            // Validation messages
            new("validation.required", MessageKeys.Validation.Required, T(MessageKeys.Validation.Required)),
            new("validation.email_invalid", MessageKeys.Validation.EmailInvalid, T(MessageKeys.Validation.EmailInvalid)),
            new("validation.min_length", MessageKeys.Validation.MinLength, T(MessageKeys.Validation.MinLength)),
            new("validation.max_length", MessageKeys.Validation.MaxLength, T(MessageKeys.Validation.MaxLength)),
            new("validation.invalid_format", MessageKeys.Validation.InvalidFormat, T(MessageKeys.Validation.InvalidFormat)),

            // Auth messages
            new("auth.invalid_credentials", MessageKeys.Auth.InvalidCredentials, T(MessageKeys.Auth.InvalidCredentials)),
            new("auth.account_inactive", MessageKeys.Auth.AccountInactive, T(MessageKeys.Auth.AccountInactive)),
            new("auth.account_locked", MessageKeys.Auth.AccountLocked, T(MessageKeys.Auth.AccountLocked)),
            new("auth.email_already_registered", MessageKeys.Auth.EmailAlreadyRegistered, T(MessageKeys.Auth.EmailAlreadyRegistered)),
            new("auth.password_reset_invalid", MessageKeys.Auth.PasswordResetInvalid, T(MessageKeys.Auth.PasswordResetInvalid)),
            new("auth.email_verify_invalid", MessageKeys.Auth.EmailVerifyInvalid, T(MessageKeys.Auth.EmailVerifyInvalid)),
            new("auth.access_denied", MessageKeys.Auth.AccessDenied, T(MessageKeys.Auth.AccessDenied)),

            // User messages
            new("user.user_not_found", MessageKeys.User.UserNotFound, T(MessageKeys.User.UserNotFound)),
        };
    }

    public class TranslationEntry
    {
        public string Category { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public TranslationEntry(string category, string key, string value)
        {
            Category = category;
            Key = key;
            Value = value;
        }
    }
}
