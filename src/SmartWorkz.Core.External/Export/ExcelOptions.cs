namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Configuration options for Excel export.
/// </summary>
public class ExcelOptions
{
    /// <summary>
    /// Gets or sets the default sheet name.
    /// </summary>
    public string SheetName { get; set; } = "Sheet1";

    /// <summary>
    /// Gets or sets whether headers should be bold.
    /// </summary>
    public bool HeaderBold { get; set; } = true;

    /// <summary>
    /// Gets or sets the header background color (hex code).
    /// </summary>
    public string HeaderBackgroundColor { get; set; } = "D3D3D3"; // Light gray

    /// <summary>
    /// Gets or sets the header font size.
    /// </summary>
    public int HeaderFontSize { get; set; } = 11;

    /// <summary>
    /// Gets or sets whether to auto-fit column widths.
    /// </summary>
    public bool AutoColumnWidth { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to freeze the first row (headers).
    /// </summary>
    public bool FreezePanes { get; set; } = true;

    /// <summary>
    /// Gets or sets the border style (thin or thick).
    /// </summary>
    public string BorderStyle { get; set; } = "thin";

    /// <summary>
    /// Gets or sets whether to center-align headers.
    /// </summary>
    public bool CenterHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the default date format.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets the default currency format.
    /// </summary>
    public string CurrencyFormat { get; set; } = "$#,##0.00";

    /// <summary>
    /// Gets or sets the number decimal places.
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;
}
