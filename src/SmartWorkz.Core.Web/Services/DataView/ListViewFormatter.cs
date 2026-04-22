namespace SmartWorkz.Web;

/// <summary>
/// Formats raw data values for display in list and grid components.
///
/// ListViewFormatter provides type-aware formatting for common data types encountered in
/// data display scenarios: dates, currency amounts, text, booleans, and generic objects.
/// It handles null values gracefully by displaying a dash (-) instead of empty/null text.
/// </summary>
/// <remarks>
/// ## Formatting Methods
/// The service provides specialized formatting methods for common types:
/// - **FormatDate**: Converts DateTime values using .NET format strings (default: "MMM dd, yyyy")
/// - **FormatCurrency**: Formats decimal amounts as currency with symbol prefix (default: "$")
/// - **TruncateText**: Limits string length with ellipsis (...) for long values
/// - **FormatBoolean**: Converts true/false to user-friendly "Yes"/"No"
/// - **FormatValue**: Dispatcher method that detects type and applies appropriate formatter
///
/// ## Type Detection and Default Formatting
/// The FormatValue method uses pattern matching to detect value types:
/// - null → "-" (dash placeholder)
/// - DateTime → FormatDate with default format
/// - decimal → FormatCurrency with default symbol
/// - bool → FormatBoolean (Yes/No)
/// - string → TruncateText with default length limit
/// - Other types → ToString() result or "-" if conversion fails
///
/// ## Null Value Handling
/// All formatting methods return "-" (dash) for null inputs. This provides a consistent,
/// user-friendly placeholder throughout the UI instead of blank or "null" text.
///
/// ## Integration with Grid/List Components
/// Components (GridComponent.razor, ListViewComponent.razor) use ListViewFormatter to prepare
/// data for display. Typically:
/// 1. Component binds a property value
/// 2. Calls formatter.FormatValue(rawValue)
/// 3. Displays the formatted string in the UI
///
/// Future integration with ViewConfiguration may filter formatting to visible columns only,
/// though current implementation formats any value independently.
///
/// ## Example Usage
/// ```csharp
/// var formatter = new ListViewFormatter();
///
/// // Dates - "Apr 22, 2026"
/// var dateStr = formatter.FormatDate(DateTime.Now);
///
/// // Currency - "$1,234.56"
/// var priceStr = formatter.FormatCurrency(1234.56m);
///
/// // Custom date format - "04/22/2026"
/// var dateStr = formatter.FormatDate(DateTime.Now, "MM/dd/yyyy");
///
/// // Text truncation - "This is a very long..."
/// var truncated = formatter.TruncateText("This is a very long product description", 20);
///
/// // Boolean - "Yes" or "No"
/// var active = formatter.FormatBoolean(true);  // "Yes"
/// var deleted = formatter.FormatBoolean(false); // "No"
///
/// // Generic dispatcher - auto-detects type
/// var formatted = formatter.FormatValue(42.50m);     // "$42.50" (decimal)
/// var formatted = formatter.FormatValue("text");     // "text" (string)
/// var formatted = formatter.FormatValue(null);       // "-" (null)
///
/// // Display in component
/// // Product price: $1,234.50 (formatted)
/// // Product name: This is a very long... (truncated)
/// // Created: Apr 22, 2026 (date)
/// ```
/// </remarks>
public class ListViewFormatter : IListViewFormatter
{
    /// <summary>
    /// Formats a date/time value using the specified .NET format string.
    /// Returns "-" if the date is null.
    /// </summary>
    /// <param name="date">The date value to format. Can be null.</param>
    /// <param name="format">The .NET format string (default: "MMM dd, yyyy" for "Apr 22, 2026").</param>
    /// <returns>Formatted date string, or "-" if date is null.</returns>
    /// <example>
    /// var formatter = new ListViewFormatter();
    /// formatter.FormatDate(new DateTime(2026, 4, 22)); // "Apr 22, 2026"
    /// formatter.FormatDate(new DateTime(2026, 4, 22), "yyyy-MM-dd"); // "2026-04-22"
    /// formatter.FormatDate(null); // "-"
    /// </example>
    public string FormatDate(DateTime? date, string format = "MMM dd, yyyy")
    {
        return date?.ToString(format) ?? "-";
    }

    /// <summary>
    /// Formats a decimal value as currency with the specified symbol prefix.
    /// Returns "-" if the value is null. Always formats to 2 decimal places.
    /// </summary>
    /// <param name="value">The decimal amount to format. Can be null.</param>
    /// <param name="currencySymbol">The currency symbol to prefix (default: "$").</param>
    /// <returns>Formatted currency string (e.g., "$1,234.50"), or "-" if value is null.</returns>
    /// <example>
    /// var formatter = new ListViewFormatter();
    /// formatter.FormatCurrency(1234.56m); // "$1,234.56"
    /// formatter.FormatCurrency(99.9m); // "$99.90"
    /// formatter.FormatCurrency(1500m, "€"); // "€1,500.00"
    /// formatter.FormatCurrency(null); // "-"
    /// </example>
    public string FormatCurrency(decimal? value, string currencySymbol = "$")
    {
        if (value == null)
            return "-";

        return $"{currencySymbol}{value:N2}";
    }

    /// <summary>
    /// Truncates text to a maximum length and appends "..." if truncated.
    /// Returns "-" if the text is null or empty.
    /// </summary>
    /// <param name="text">The text to truncate. Can be null or empty.</param>
    /// <param name="maxLength">Maximum length before truncation (default: 100 characters).</param>
    /// <returns>Truncated text with "..." appended if over max length, "-" if null/empty, or original text if shorter.</returns>
    /// <example>
    /// var formatter = new ListViewFormatter();
    /// formatter.TruncateText("Short", 20); // "Short"
    /// formatter.TruncateText("This is a very long product description", 20); // "This is a very long ..."
    /// formatter.TruncateText("", 20); // "-"
    /// formatter.TruncateText(null, 20); // "-"
    /// </example>
    public string TruncateText(string? text, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(text))
            return "-";

        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }

    /// <summary>
    /// Formats a boolean value as human-readable text.
    /// Returns "Yes" for true, "No" for false, and "-" for null.
    /// </summary>
    /// <param name="value">The boolean value to format. Can be null.</param>
    /// <returns>"Yes" for true, "No" for false, "-" for null.</returns>
    /// <example>
    /// var formatter = new ListViewFormatter();
    /// formatter.FormatBoolean(true); // "Yes"
    /// formatter.FormatBoolean(false); // "No"
    /// formatter.FormatBoolean(null); // "-"
    /// </example>
    public string FormatBoolean(bool? value)
    {
        return value switch
        {
            true => "Yes",
            false => "No",
            null => "-"
        };
    }

    /// <summary>
    /// Formats any object value using type-aware detection and appropriate formatter.
    /// Uses pattern matching to dispatch to specialized formatters:
    /// - null → "-"
    /// - DateTime → FormatDate with default format
    /// - decimal → FormatCurrency with default symbol
    /// - bool → FormatBoolean (Yes/No)
    /// - string → TruncateText with default limit
    /// - Other → ToString() or "-" if ToString returns null
    /// </summary>
    /// <param name="value">The value to format. Can be any type or null.</param>
    /// <returns>Formatted string appropriate to the value's type, or "-" if null or formatting fails.</returns>
    /// <example>
    /// var formatter = new ListViewFormatter();
    /// formatter.FormatValue(49.50m); // "$49.50" (decimal dispatches to FormatCurrency)
    /// formatter.FormatValue(new DateTime(2026, 4, 22)); // "Apr 22, 2026" (DateTime to FormatDate)
    /// formatter.FormatValue(true); // "Yes" (bool to FormatBoolean)
    /// formatter.FormatValue("Some text"); // "Some text" (string to TruncateText)
    /// formatter.FormatValue(42); // "42" (int falls through to ToString)
    /// formatter.FormatValue(null); // "-" (null returns dash)
    /// </example>
    public string FormatValue(object? value)
    {
        return value switch
        {
            null => "-",
            DateTime dateTime => FormatDate(dateTime),
            decimal decimalValue => FormatCurrency(decimalValue),
            bool boolValue => FormatBoolean(boolValue),
            string stringValue => TruncateText(stringValue),
            _ => value.ToString() ?? "-"
        };
    }
}
