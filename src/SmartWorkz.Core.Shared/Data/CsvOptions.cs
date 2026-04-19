namespace SmartWorkz.Core.Shared.Data;

/// <summary>
/// Configuration options for CSV read/write operations.
/// Sealed to prevent inheritance and ensure consistent behavior.
/// </summary>
public sealed class CsvOptions
{
    /// <summary>
    /// Gets or sets the delimiter character used to separate columns.
    /// Default is comma (,).
    /// </summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets the character used to quote fields containing special characters.
    /// Default is double quote (").
    /// </summary>
    public char QuoteChar { get; set; } = '"';

    /// <summary>
    /// Gets or sets a value indicating whether the first row contains column headers.
    /// Default is true.
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trim whitespace from field values.
    /// Default is true.
    /// </summary>
    public bool TrimValues { get; set; } = true;

    /// <summary>
    /// Creates a default instance of CsvOptions.
    /// </summary>
    public CsvOptions()
    {
    }

    /// <summary>
    /// Creates an instance of CsvOptions with specified delimiter and quote character.
    /// </summary>
    /// <param name="delimiter">The field delimiter character.</param>
    /// <param name="quoteChar">The quote character for quoted fields.</param>
    public CsvOptions(char delimiter, char quoteChar)
    {
        Delimiter = delimiter;
        QuoteChar = quoteChar;
    }
}
