namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Configuration options for CSV export operations.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Gets or sets the delimiter character used to separate CSV fields.
    /// Default is comma (,).
    /// </summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets whether to include header row in the CSV export.
    /// Default is true.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the encoding to use for the CSV export.
    /// Default is UTF-8.
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;

    /// <summary>
    /// Gets or sets whether to use CultureInfo.CurrentCulture for value formatting.
    /// Default is false (uses invariant culture).
    /// </summary>
    public bool UseCultureInfo { get; set; } = false;
}
