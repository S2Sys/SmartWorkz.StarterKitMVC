using System.Text.RegularExpressions;

namespace SmartWorkz.Core;

/// <summary>
/// Immutable value object representing a valid email address.
/// </summary>
/// <remarks>
/// Domain-driven design value object (equality by value, immutable).
/// - Immutable: Email value cannot change after creation
/// - Validated: Email format is validated against standard pattern; must be 1-256 characters; stored in lowercase
/// - Type-Safe: Treats email as single atomic value; two emails are equal only if exact values match (case-insensitive)
/// - Business Meaning: Represents a unique contact point for customer communication and account identification
/// - Normalization: Email is automatically trimmed and converted to lowercase for consistent comparison
/// </remarks>
/// <example>
/// <code>
/// var emailResult = EmailAddress.Create("customer@example.com");
///
/// if (emailResult.IsSuccess)
/// {
///     var email = emailResult.Value;
///     Console.WriteLine(email.Value);  // "customer@example.com"
///     customer.Email = email;
/// }
/// else
/// {
///     // Handle validation error
///     switch (emailResult.Error.Code)
///     {
///         case "EMAIL_EMPTY":
///             logger.LogError("Email address is required");
///             break;
///         case "EMAIL_INVALID":
///             logger.LogError("Email format is invalid");
///             break;
///         case "EMAIL_TOO_LONG":
///             logger.LogError("Email must not exceed 256 characters");
///             break;
///     }
/// }
///
/// // Email equality is case-insensitive (normalized during creation)
/// var email1 = EmailAddress.Create("John@Example.COM").Value;
/// var email2 = EmailAddress.Create("john@example.com").Value;
/// bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
/// </code>
/// </example>
public sealed class EmailAddress : ValueObject
{
    /// <summary>
    /// Email validation pattern: basic format check for "user@domain.extension"
    /// </summary>
    /// <remarks>
    /// Pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
    /// Requires: at least one non-@ character, followed by @, followed by at least one non-@ character,
    /// followed by a dot, followed by at least one non-@ character.
    /// Does not validate against RFC 5322 (full spec); provides practical validation for common cases.
    /// </remarks>
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>
    /// Initializes a new EmailAddress with a validated, normalized email.
    /// Constructor is private; use Create() factory method to construct instances.
    /// </summary>
    /// <param name="value">Trimmed, lowercase email address</param>
    private EmailAddress(string value) => Value = value;

    /// <summary>
    /// The email address value (normalized to lowercase).
    /// </summary>
    /// <remarks>
    /// Always lowercase and trimmed. Never null.
    /// Examples: "customer@example.com", "support@domain.org"
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Factory method to create a validated EmailAddress value object.
    /// </summary>
    /// <param name="email">Email address string (required, non-empty, valid format)</param>
    /// <returns>
    /// Success result containing the EmailAddress if all validations pass.
    /// Failure result with specific error code if validation fails.
    /// </returns>
    /// <remarks>
    /// Validation rules (applied in order):
    /// 1. Email must not be null, empty, or whitespace-only
    /// 2. Email must match standard format pattern (user@domain.extension)
    /// 3. Email must not exceed 256 characters
    ///
    /// Email is normalized to lowercase and trimmed before storage.
    ///
    /// Possible error codes:
    /// - EMAIL_EMPTY: Email is null, empty, or whitespace-only
    /// - EMAIL_INVALID: Email format does not match pattern (missing @, no domain, etc.)
    /// - EMAIL_TOO_LONG: Email exceeds 256 characters
    /// </remarks>
    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email address cannot be empty"));

        var trimmed = email.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(trimmed, EmailPattern))
            return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email address format is invalid"));

        if (trimmed.Length > 256)
            return Result.Fail<EmailAddress>(new Error("EMAIL_TOO_LONG", "Email address must not exceed 256 characters"));

        return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
    }

    /// <summary>
    /// Returns the atomic value that defines this email's identity.
    /// </summary>
    /// <returns>The normalized email address string</returns>
    /// <remarks>
    /// Used for value-based equality comparison. Two emails are equal if their normalized values match exactly.
    /// </remarks>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the email address as a string.
    /// </summary>
    /// <returns>The normalized email address (lowercase)</returns>
    public override string ToString() => Value;
}


