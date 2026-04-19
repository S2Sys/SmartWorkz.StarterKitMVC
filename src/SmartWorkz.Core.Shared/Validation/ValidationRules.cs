namespace SmartWorkz.Core.Shared.Validation;

/// <summary>
/// Pre-built validation rules for common scenarios.
/// </summary>
public static class ValidationRules
{
    /// <summary>Email address regex pattern (RFC 5322 simplified).</summary>
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    /// <summary>URL regex pattern.</summary>
    public const string UrlPattern = @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$";

    /// <summary>Phone number pattern (10-15 digits).</summary>
    public const string PhonePattern = @"^\d{10,15}$";

    /// <summary>Strong password pattern (at least 8 chars, uppercase, number, special).</summary>
    public const string StrongPasswordPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,}$";

    /// <summary>Postal code pattern (US).</summary>
    public const string PostalCodePattern = @"^\d{5}(-\d{4})?$";
}
