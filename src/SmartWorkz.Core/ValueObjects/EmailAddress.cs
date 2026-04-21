using System.Text.RegularExpressions;

namespace SmartWorkz.Core;

public sealed class EmailAddress : ValueObject
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

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

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}


