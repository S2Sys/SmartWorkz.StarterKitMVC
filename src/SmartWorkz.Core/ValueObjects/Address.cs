namespace SmartWorkz.Core;

/// <summary>
/// Immutable value object representing a physical mailing address.
/// </summary>
/// <remarks>
/// Domain-driven design value object (equality by value, immutable).
/// - Immutable: All address components (street, city, state, postal code, country) cannot change after creation
/// - Validated: All required fields must be non-empty; whitespace is trimmed during creation
/// - Type-Safe: Treats all address components as a single indivisible unit; two addresses are equal only if all components match
/// - Business Meaning: Represents a specific geographic location for shipping, billing, or customer records
/// - Equality: Two addresses are equal if all five components match exactly (street, city, state, postal code, country)
/// </remarks>
/// <example>
/// <code>
/// var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
///
/// if (addressResult.IsSuccess)
/// {
///     var address = addressResult.Value;
///     Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
///     order.ShippingAddress = address;
/// }
/// else
/// {
///     // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
///     logger.LogError(addressResult.Error.Message);
/// }
///
/// // Address equality is value-based
/// var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
/// var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
/// bool areEqual = address1 == address2;  // true (same address values)
/// </code>
/// </example>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Initializes a new Address with validated components.
    /// Constructor is private; use Create() factory method to construct instances.
    /// </summary>
    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// The street address line.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "123 Main Street", "Suite 100, 456 Oak Avenue"
    /// </remarks>
    public string Street { get; }

    /// <summary>
    /// The city or municipality name.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "Springfield", "New York", "Los Angeles"
    /// </remarks>
    public string City { get; }

    /// <summary>
    /// The state, province, or region code.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "IL", "CA", "ON", "AB"
    /// </remarks>
    public string State { get; }

    /// <summary>
    /// The postal code, ZIP code, or equivalent.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// No specific format validation; accepts any postal code format.
    /// Examples: "62701", "90210", "M5V 3A8"
    /// </remarks>
    public string PostalCode { get; }

    /// <summary>
    /// The country name or code.
    /// </summary>
    /// <remarks>
    /// Required, non-empty string. Whitespace is trimmed during creation.
    /// Examples: "USA", "Canada", "Mexico"
    /// </remarks>
    public string Country { get; }

    /// <summary>
    /// Formatted full address combining all components.
    /// </summary>
    /// <remarks>
    /// Format: "{Street}, {City}, {State} {PostalCode}, {Country}"
    /// Example: "123 Main St, Springfield, IL 62701, USA"
    /// </remarks>
    public string FullAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";

    /// <summary>
    /// Factory method to create a validated Address value object.
    /// </summary>
    /// <param name="street">Street address (required, non-empty)</param>
    /// <param name="city">City or municipality name (required, non-empty)</param>
    /// <param name="state">State, province, or region (required, non-empty)</param>
    /// <param name="postalCode">Postal/ZIP code (required, non-empty)</param>
    /// <param name="country">Country name or code (required, non-empty)</param>
    /// <returns>
    /// Success result containing the Address if all validations pass.
    /// Failure result with specific error code if any component is empty or null.
    /// </returns>
    /// <remarks>
    /// All fields are required and must be non-empty. Whitespace is automatically trimmed.
    /// Possible error codes:
    /// - STREET_EMPTY: Street address is null, empty, or whitespace-only
    /// - CITY_EMPTY: City is null, empty, or whitespace-only
    /// - STATE_EMPTY: State is null, empty, or whitespace-only
    /// - POSTAL_CODE_EMPTY: Postal code is null, empty, or whitespace-only
    /// - COUNTRY_EMPTY: Country is null, empty, or whitespace-only
    /// </remarks>
    public static Result<Address> Create(string? street, string? city, string? state, string? postalCode, string? country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Fail<Address>(new Error("STREET_EMPTY", "Street address cannot be empty"));

        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail<Address>(new Error("CITY_EMPTY", "City cannot be empty"));

        if (string.IsNullOrWhiteSpace(state))
            return Result.Fail<Address>(new Error("STATE_EMPTY", "State/Province cannot be empty"));

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Fail<Address>(new Error("POSTAL_CODE_EMPTY", "Postal code cannot be empty"));

        if (string.IsNullOrWhiteSpace(country))
            return Result.Fail<Address>(new Error("COUNTRY_EMPTY", "Country cannot be empty"));

        return Result.Ok<Address>(new Address(
            street.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode.Trim(),
            country.Trim()
        ));
    }

    /// <summary>
    /// Returns the atomic values that define this address's identity.
    /// </summary>
    /// <returns>All address components in order: street, city, state, postal code, country</returns>
    /// <remarks>
    /// Used for value-based equality comparison. Two addresses are equal if all atomic values match.
    /// </remarks>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    /// <summary>
    /// Returns the formatted full address string.
    /// </summary>
    /// <returns>Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"</returns>
    /// <remarks>
    /// Example: "123 Main St, Springfield, IL 62701, USA"
    /// </remarks>
    public override string ToString() => FullAddress;
}


