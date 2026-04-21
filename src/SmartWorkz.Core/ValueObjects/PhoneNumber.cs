using System.Text.RegularExpressions;

namespace SmartWorkz.Core;

public sealed class PhoneNumber : ValueObject
{
    private PhoneNumber(string formattedNumber)
    {
        FormattedNumber = formattedNumber;
    }

    public string FormattedNumber { get; }

    public static Result<PhoneNumber> Create(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result<PhoneNumber>.Failure(new Error("PHONE_EMPTY", "Phone number cannot be empty"));

        var digitsOnly = Regex.Replace(phone, @"\D", "");

        if (digitsOnly.Length < 10)
            return Result<PhoneNumber>.Failure(new Error("PHONE_TOO_SHORT", "Phone number must contain at least 10 digits"));

        if (digitsOnly.Length > 15)
            return Result<PhoneNumber>.Failure(new Error("PHONE_TOO_LONG", "Phone number must not exceed 15 digits"));

        return Result<PhoneNumber>.Success(new PhoneNumber(digitsOnly));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FormattedNumber;
    }

    public override string ToString() => FormattedNumber;
}
