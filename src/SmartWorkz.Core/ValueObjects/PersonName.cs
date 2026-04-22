namespace SmartWorkz.Core;

/// <summary>
/// Immutable value object representing a person's name with optional middle name.
/// </summary>
/// <remarks>
/// Domain-driven design value object (equality by value, immutable).
/// - Immutable: First name, last name, and middle name cannot change after creation
/// - Validated: First and last names are required; middle name is optional but must be non-empty if provided; whitespace is trimmed
/// - Type-Safe: Treats all name components as single indivisible unit; two names are equal only if all components match exactly
/// - Business Meaning: Represents a person's legal or display name for customer records, employee data, and user profiles
/// - Flexible: Middle name is optional, supporting naming conventions across different cultures
/// </remarks>
/// <example>
/// <code>
/// // Creating a name without middle name
/// var nameResult = PersonName.Create("John", "Doe");
/// if (nameResult.IsSuccess)
/// {
///     customer.Name = nameResult.Value;  // "John Doe"
/// }
///
/// // Creating a name with middle name
/// var fullNameResult = PersonName.Create("John", "Doe", "Michael");
/// if (fullNameResult.IsSuccess)
/// {
///     customer.Name = fullNameResult.Value;  // "John Michael Doe"
/// }
/// else
/// {
///     logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
/// }
///
/// // Accessing name components
/// var name = PersonName.Create("Jane", "Smith", "Marie").Value;
/// Console.WriteLine(name.FirstName);   // "Jane"
/// Console.WriteLine(name.MiddleName);  // "Marie"
/// Console.WriteLine(name.LastName);    // "Smith"
/// Console.WriteLine(name.FullName);    // "Jane Marie Smith"
///
/// // Name equality is value-based
/// var name1 = PersonName.Create("John", "Doe").Value;
/// var name2 = PersonName.Create("John", "Doe").Value;
/// bool areEqual = name1 == name2;  // true (same name values)
/// </code>
/// </example>
public sealed class PersonName : ValueObject
{
    /// <summary>
    /// Initializes a new PersonName with required first and last names, and optional middle name.
    /// Constructor is private; use Create() factory method to construct instances.
    /// </summary>
    /// <param name="firstName">The person's first name (required, non-empty)</param>
    /// <param name="lastName">The person's last name (required, non-empty)</param>
    /// <param name="middleName">The person's middle name (optional; null if not provided)</param>
    private PersonName(string firstName, string lastName, string? middleName = null)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    /// <summary>
    /// The person's first (given) name.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "John", "Jane", "Robert"
    /// </remarks>
    public string FirstName { get; }

    /// <summary>
    /// The person's last (family) name.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "Doe", "Smith", "Johnson"
    /// </remarks>
    public string LastName { get; }

    /// <summary>
    /// The person's middle name (optional).
    /// </summary>
    /// <remarks>
    /// Optional field; can be null or empty. If provided, whitespace is trimmed during creation.
    /// Examples: "Michael", "Anne", "Joseph"
    /// </remarks>
    public string? MiddleName { get; }

    /// <summary>
    /// The person's complete name combining all components.
    /// </summary>
    /// <remarks>
    /// Format depends on whether middle name is present:
    /// - Without middle name: "{FirstName} {LastName}" (e.g., "John Doe")
    /// - With middle name: "{FirstName} {MiddleName} {LastName}" (e.g., "John Michael Doe")
    /// </remarks>
    public string FullName =>
        string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

    /// <summary>
    /// Factory method to create a validated PersonName value object.
    /// </summary>
    /// <param name="firstName">The person's first name (required, non-empty)</param>
    /// <param name="lastName">The person's last name (required, non-empty)</param>
    /// <param name="middleName">The person's middle name (optional; null or empty is allowed)</param>
    /// <returns>
    /// Success result containing the PersonName if all validations pass.
    /// Failure result with specific error code if any required field is missing.
    /// </returns>
    /// <remarks>
    /// Validation rules (applied in order):
    /// 1. First name must not be null, empty, or whitespace-only
    /// 2. Last name must not be null, empty, or whitespace-only
    /// 3. Middle name is optional; if provided and non-empty, it is included; null/empty inputs store as null
    ///
    /// All provided names are trimmed of leading/trailing whitespace.
    ///
    /// Possible error codes:
    /// - FIRST_NAME_EMPTY: First name is null, empty, or whitespace-only
    /// - LAST_NAME_EMPTY: Last name is null, empty, or whitespace-only
    /// </remarks>
    public static Result<PersonName> Create(string? firstName, string? lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Fail<PersonName>(new Error("FIRST_NAME_EMPTY", "First name cannot be empty"));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Fail<PersonName>(new Error("LAST_NAME_EMPTY", "Last name cannot be empty"));

        return Result.Ok<PersonName>(new PersonName(
            firstName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim()
        ));
    }

    /// <summary>
    /// Returns the atomic values that define this name's identity.
    /// </summary>
    /// <returns>First name, middle name (or empty string if null), and last name in order</returns>
    /// <remarks>
    /// Used for value-based equality comparison. Two names are equal if all three components match.
    /// Middle name is normalized to empty string for hashing purposes to handle null values consistently.
    /// </remarks>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FirstName;
        yield return MiddleName ?? string.Empty;
        yield return LastName;
    }

    /// <summary>
    /// Returns the formatted full name.
    /// </summary>
    /// <returns>Complete name including middle name if present</returns>
    /// <remarks>
    /// Format depends on middle name presence:
    /// - "John Doe" (no middle name)
    /// - "John Michael Doe" (with middle name)
    /// </remarks>
    public override string ToString() => FullName;
}


