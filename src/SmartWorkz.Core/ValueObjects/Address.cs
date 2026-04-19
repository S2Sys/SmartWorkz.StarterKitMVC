using SmartWorkz.Core.Results;

namespace SmartWorkz.Core.ValueObjects;

public sealed class Address : ValueObject
{
    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public string FullAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";

    public static Result<Address> Create(string? street, string? city, string? state, string? postalCode, string? country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure(new Error("STREET_EMPTY", "Street address cannot be empty"));

        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure(new Error("CITY_EMPTY", "City cannot be empty"));

        if (string.IsNullOrWhiteSpace(state))
            return Result<Address>.Failure(new Error("STATE_EMPTY", "State/Province cannot be empty"));

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result<Address>.Failure(new Error("POSTAL_CODE_EMPTY", "Postal code cannot be empty"));

        if (string.IsNullOrWhiteSpace(country))
            return Result<Address>.Failure(new Error("COUNTRY_EMPTY", "Country cannot be empty"));

        return Result<Address>.Success(new Address(
            street.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode.Trim(),
            country.Trim()
        ));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => FullAddress;
}
