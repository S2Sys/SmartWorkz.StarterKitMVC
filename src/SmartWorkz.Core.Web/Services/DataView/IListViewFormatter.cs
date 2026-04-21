namespace SmartWorkz.Web;

/// <summary>
/// Formats data for List/Card view display (dates, currency, text truncation, etc).
/// </summary>
public interface IListViewFormatter
{
    /// <summary>Format a date value for display.</summary>
    string FormatDate(DateTime? date, string format = "MMM dd, yyyy");

    /// <summary>Format a decimal value as currency.</summary>
    string FormatCurrency(decimal? value, string currencySymbol = "$");

    /// <summary>Truncate text to max length with ellipsis.</summary>
    string TruncateText(string? text, int maxLength = 100);

    /// <summary>Format a boolean as human-readable text.</summary>
    string FormatBoolean(bool? value);

    /// <summary>Format any object using type-aware rules.</summary>
    string FormatValue(object? value);
}
