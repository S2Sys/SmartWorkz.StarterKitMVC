using System.Text.RegularExpressions;

namespace SmartWorkz.Core;

/// <summary>
/// Immutable value object representing a phone number with digit validation.
/// </summary>
/// <remarks>
/// Domain-driven design value object (equality by value, immutable).
/// - Immutable: Phone number cannot change after creation
/// - Validated: Extracts digits only; must contain 10-15 digits (E.164 standard range)
/// - Type-Safe: Stores and compares based on digit sequence; formatting is normalized to digits-only
/// - Business Meaning: Represents a contact telephone number for customer communication and account verification
/// - Format-Agnostic: Accepts any format (with hyphens, spaces, parentheses) and normalizes to digits
/// </remarks>
/// <example>
/// <code>
/// // Creating a phone number with various formats
/// var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
/// if (phoneResult.IsSuccess)
/// {
///     customer.Phone = phoneResult.Value;  // Stores as "5551234567"
///     Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
/// }
/// else
/// {
///     logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
/// }
///
/// // More format examples
/// var validFormats = new[]
/// {
///     "555-123-4567",      // With hyphens
///     "(555) 123-4567",    // With parentheses and spaces
///     "5551234567",        // Digits only
///     "+1 555 123 4567",   // With plus and spaces
///     "555.123.4567"       // With dots
/// };
///
/// // Phone number equality is based on digits
/// var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
/// var phone2 = PhoneNumber.Create("555-123-4567").Value;
/// bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
/// </code>
/// </example>
public sealed class PhoneNumber : ValueObject
{
    /// <summary>
    /// Initializes a new PhoneNumber with a validated, normalized digit sequence.
    /// Constructor is private; use Create() factory method to construct instances.
    /// </summary>
    /// <param name="formattedNumber">The phone number as digits only (10-15 digits)</param>
    private PhoneNumber(string formattedNumber)
    {
        FormattedNumber = formattedNumber;
    }

    /// <summary>
    /// The phone number as a digit-only string.
    /// </summary>
    /// <remarks>
    /// Contains only digits (0-9); all formatting characters (hyphens, spaces, parentheses, etc.) are removed.
    /// Length is between 10 and 15 digits (E.164 international standard range).
    /// Examples: "5551234567", "14155552671", "442071838750"
    /// </remarks>
    public string FormattedNumber { get; }

    /// <summary>
    /// Factory method to create a validated PhoneNumber value object.
    /// </summary>
    /// <param name="phone">Phone number in any format (required, non-empty)</param>
    /// <returns>
    /// Success result containing the PhoneNumber if all validations pass.
    /// Failure result with specific error code if any validation fails.
    /// </returns>
    /// <remarks>
    /// Validation rules (applied in order):
    /// 1. Phone must not be null, empty, or whitespace-only
    /// 2. After extracting digits only, must contain at least 10 digits (minimum valid phone length)
    /// 3. After extracting digits only, must not exceed 15 digits (E.164 international standard maximum)
    ///
    /// Input can be in any format with hyphens, spaces, parentheses, plus sign, dots, etc.
    /// All non-digit characters are automatically removed during validation.
    ///
    /// Possible error codes:
    /// - PHONE_EMPTY: Phone is null, empty, or whitespace-only
    /// - PHONE_TOO_SHORT: Digit count is less than 10
    /// - PHONE_TOO_LONG: Digit count exceeds 15
    /// </remarks>
    public static Result<PhoneNumber> Create(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Fail<PhoneNumber>(new Error("PHONE_EMPTY", "Phone number cannot be empty"));

        var digitsOnly = Regex.Replace(phone, @"\D", "");

        if (digitsOnly.Length < 10)
            return Result.Fail<PhoneNumber>(new Error("PHONE_TOO_SHORT", "Phone number must contain at least 10 digits"));

        if (digitsOnly.Length > 15)
            return Result.Fail<PhoneNumber>(new Error("PHONE_TOO_LONG", "Phone number must not exceed 15 digits"));

        return Result.Ok<PhoneNumber>(new PhoneNumber(digitsOnly));
    }

    /// <summary>
    /// Returns the atomic value that defines this phone number's identity.
    /// </summary>
    /// <returns>The digit-only phone number string</returns>
    /// <remarks>
    /// Used for value-based equality comparison. Two phone numbers are equal if their digit sequences match exactly.
    /// </remarks>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FormattedNumber;
    }

    /// <summary>
    /// Returns the phone number as a digit-only string.
    /// </summary>
    /// <returns>The phone number containing only digits (0-9)</returns>
    /// <remarks>
    /// Example: "5551234567"
    /// </remarks>
    public override string ToString() => FormattedNumber;
}


