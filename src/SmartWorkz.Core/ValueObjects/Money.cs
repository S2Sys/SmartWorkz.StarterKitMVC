using SmartWorkz.Core.Shared.Results;

namespace SmartWorkz.Core.ValueObjects;

public sealed class Money : ValueObject
{
    private static readonly HashSet<string> ValidCurrencies = new() { "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "INR", "CNY" };

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (amount < 0)
            return Result<Money>.Fail(new Error("MONEY_NEGATIVE", "Amount cannot be negative"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result<Money>.Fail(new Error("CURRENCY_EMPTY", "Currency code cannot be empty"));

        var currencyCode = currency.Trim().ToUpperInvariant();

        if (!ValidCurrencies.Contains(currencyCode))
            return Result<Money>.Fail(new Error("CURRENCY_INVALID", $"Currency '{currencyCode}' is not supported"));

        return Result<Money>.Ok(new Money(amount, currencyCode));
    }

    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Fail(new Error("CURRENCY_MISMATCH", "Cannot add money with different currencies"));

        return Result<Money>.Ok(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Fail(new Error("CURRENCY_MISMATCH", "Cannot subtract money with different currencies"));

        var result = Amount - other.Amount;
        if (result < 0)
            return Result<Money>.Fail(new Error("INSUFFICIENT_FUNDS", "Resulting amount would be negative"));

        return Result<Money>.Ok(new Money(result, Currency));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
