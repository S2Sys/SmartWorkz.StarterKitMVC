namespace SmartWorkz.Core;

public sealed class PersonName : ValueObject
{
    private PersonName(string firstName, string lastName, string? middleName = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }

    public string FullName =>
        string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

    public static Result<PersonName> Create(string? firstName, string? lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<PersonName>.Failure(new Error("FIRST_NAME_EMPTY", "First name cannot be empty"));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<PersonName>.Failure(new Error("LAST_NAME_EMPTY", "Last name cannot be empty"));

        return Result<PersonName>.Success(new PersonName(
            firstName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim()
        ));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FirstName;
        yield return MiddleName ?? string.Empty;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
