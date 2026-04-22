namespace SmartWorkz.Core;

/// <summary>
/// Immutable value object representing a monetary amount with currency validation.
/// </summary>
/// <remarks>
/// Domain-driven design value object (equality by value, immutable).
/// - Immutable: Amount and Currency cannot change after creation
/// - Validated: Only non-negative amounts; currency must be one of the whitelisted ISO 4217 codes (USD, EUR, GBP, JPY, CAD, AUD, INR, CNY)
/// - Type-Safe: Treats amount and currency as single indivisible unit; arithmetic operations validate currency matches
/// - Atomic: Cannot perform calculations between different currencies; ensures monetary consistency in domain operations
/// - Currency Safe: Add() and Subtract() methods validate that both operands use the same currency before calculation
/// </remarks>
/// <example>
/// <code>
/// // Creating a money value
/// var priceResult = Money.Create(99.99m, "USD");
/// if (priceResult.IsSuccess)
/// {
///     order.Total = priceResult.Value;  // 99.99 USD
/// }
/// else
/// {
///     logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
/// }
///
/// // Performing arithmetic operations
/// var cost = Money.Create(50.00m, "USD").Value;
/// var tax = Money.Create(5.00m, "USD").Value;
/// var totalResult = cost.Add(tax);
///
/// if (totalResult.IsSuccess)
/// {
///     order.Total = totalResult.Value;  // 55.00 USD
/// }
/// else
/// {
///     logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
/// }
///
/// // Attempting to mix currencies fails safely
/// var usd = Money.Create(100m, "USD").Value;
/// var eur = Money.Create(100m, "EUR").Value;
/// var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
/// </code>
/// </example>
public sealed class Money : ValueObject
{
    /// <summary>
    /// Whitelist of valid ISO 4217 currency codes.
    /// </summary>
    /// <remarks>
    /// Supported currencies: USD, EUR, GBP, JPY, CAD, AUD, INR, CNY
    /// This is a finite set to ensure monetary consistency across the domain.
    /// Additional currencies can be added if business requirements change.
    /// </remarks>
    private static readonly HashSet<string> ValidCurrencies = new() { "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "INR", "CNY" };

    /// <summary>
    /// Initializes a new Money value object with amount and currency.
    /// Constructor is private; use Create() factory method to construct instances.
    /// </summary>
    /// <param name="amount">The monetary amount (must be non-negative)</param>
    /// <param name="currency">The ISO 4217 currency code (uppercase)</param>
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// The monetary amount.
    /// </summary>
    /// <remarks>
    /// Always non-negative. Uses decimal type for financial precision.
    /// Examples: 99.99, 0.50, 1000.00
    /// </remarks>
    public decimal Amount { get; }

    /// <summary>
    /// The ISO 4217 currency code in uppercase.
    /// </summary>
    /// <remarks>
    /// Must be one of the whitelisted currencies: USD, EUR, GBP, JPY, CAD, AUD, INR, CNY
    /// Stored in uppercase for consistent comparison.
    /// </remarks>
    public string Currency { get; }

    /// <summary>
    /// Factory method to create a validated Money value object.
    /// </summary>
    /// <param name="amount">The monetary amount (must be non-negative)</param>
    /// <param name="currency">The ISO 4217 currency code (required, non-empty)</param>
    /// <returns>
    /// Success result containing the Money if all validations pass.
    /// Failure result with specific error code if any validation fails.
    /// </returns>
    /// <remarks>
    /// Validation rules (applied in order):
    /// 1. Amount must not be negative
    /// 2. Currency code must not be null, empty, or whitespace-only
    /// 3. Currency code must be one of the whitelisted ISO 4217 codes (USD, EUR, GBP, JPY, CAD, AUD, INR, CNY)
    ///
    /// Currency code is normalized to uppercase before storage.
    ///
    /// Possible error codes:
    /// - MONEY_NEGATIVE: Amount is less than zero
    /// - CURRENCY_EMPTY: Currency is null, empty, or whitespace-only
    /// - CURRENCY_INVALID: Currency code is not in the whitelist
    /// </remarks>
    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (amount < 0)
            return Result.Fail<Money>(new Error("MONEY_NEGATIVE", "Amount cannot be negative"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Fail<Money>(new Error("CURRENCY_EMPTY", "Currency code cannot be empty"));

        var currencyCode = currency.Trim().ToUpperInvariant();

        if (!ValidCurrencies.Contains(currencyCode))
            return Result.Fail<Money>(new Error("CURRENCY_INVALID", $"Currency '{currencyCode}' is not supported"));

        return Result.Ok<Money>(new Money(amount, currencyCode));
    }

    /// <summary>
    /// Adds two monetary amounts if they share the same currency.
    /// </summary>
    /// <param name="other">Another Money value object to add</param>
    /// <returns>
    /// Success result containing the sum if currencies match.
    /// Failure result if currencies differ.
    /// </returns>
    /// <remarks>
    /// Currency must match exactly. This prevents accidental mixing of different currencies.
    /// Example: 100 USD + 50 USD = 150 USD
    /// Example: 100 USD + 50 EUR = Error (CURRENCY_MISMATCH)
    ///
    /// Possible error codes:
    /// - CURRENCY_MISMATCH: Operands have different currency codes
    /// </remarks>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result.Fail<Money>(new Error("CURRENCY_MISMATCH", "Cannot add money with different currencies"));

        return Result.Ok<Money>(new Money(Amount + other.Amount, Currency));
    }

    /// <summary>
    /// Subtracts another monetary amount from this one if they share the same currency.
    /// </summary>
    /// <param name="other">Another Money value object to subtract</param>
    /// <returns>
    /// Success result containing the difference if currencies match and result is non-negative.
    /// Failure result if currencies differ or result would be negative.
    /// </returns>
    /// <remarks>
    /// Currency must match exactly. Result must not be negative.
    /// Example: 150 USD - 50 USD = 100 USD
    /// Example: 50 USD - 100 USD = Error (INSUFFICIENT_FUNDS)
    /// Example: 100 USD - 50 EUR = Error (CURRENCY_MISMATCH)
    ///
    /// Possible error codes:
    /// - CURRENCY_MISMATCH: Operands have different currency codes
    /// - INSUFFICIENT_FUNDS: Subtraction would result in a negative amount
    /// </remarks>
    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return Result.Fail<Money>(new Error("CURRENCY_MISMATCH", "Cannot subtract money with different currencies"));

        var result = Amount - other.Amount;
        if (result < 0)
            return Result.Fail<Money>(new Error("INSUFFICIENT_FUNDS", "Resulting amount would be negative"));

        return Result.Ok<Money>(new Money(result, Currency));
    }

    /// <summary>
    /// Returns the atomic values that define this money's identity.
    /// </summary>
    /// <returns>Both amount and currency in order</returns>
    /// <remarks>
    /// Used for value-based equality comparison. Two money values are equal only if both amount AND currency match.
    /// Example: 100 USD == 100 USD (true), but 100 USD != 100 EUR (false)
    /// </remarks>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Returns the formatted monetary amount with currency.
    /// </summary>
    /// <returns>Amount formatted to 2 decimal places, followed by currency code</returns>
    /// <remarks>
    /// Format: "{Amount:F2} {Currency}"
    /// Examples: "99.99 USD", "0.50 EUR", "1000.00 GBP"
    /// </remarks>
    public override string ToString() => $"{Amount:F2} {Currency}";
}


