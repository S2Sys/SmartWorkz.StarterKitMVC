namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Configuration options for PDF export.
/// </summary>
public class PdfOptions
{
    /// <summary>
    /// Gets or sets the page size (A4, Letter, etc).
    /// </summary>
    public string PageSize { get; set; } = "A4";

    /// <summary>
    /// Gets or sets the page orientation (Portrait or Landscape).
    /// </summary>
    public string Orientation { get; set; } = "Portrait";

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether to include page numbers.
    /// </summary>
    public bool IncludePageNumbers { get; set; } = true;

    /// <summary>
    /// Gets or sets the top margin in points.
    /// </summary>
    public float TopMargin { get; set; } = 36; // 0.5 inch

    /// <summary>
    /// Gets or sets the bottom margin in points.
    /// </summary>
    public float BottomMargin { get; set; } = 36; // 0.5 inch

    /// <summary>
    /// Gets or sets the left margin in points.
    /// </summary>
    public float LeftMargin { get; set; } = 36; // 0.5 inch

    /// <summary>
    /// Gets or sets the right margin in points.
    /// </summary>
    public float RightMargin { get; set; } = 36; // 0.5 inch

    /// <summary>
    /// Gets or sets whether headers should be bold.
    /// </summary>
    public bool HeaderBold { get; set; } = true;

    /// <summary>
    /// Gets or sets the header background color (RGB).
    /// </summary>
    public object? HeaderBackgroundColor { get; set; } // Color representation for PDF header

    /// <summary>
    /// Gets or sets the default date format.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets the default currency format.
    /// </summary>
    public string CurrencyFormat { get; set; } = "C";

    /// <summary>
    /// Gets or sets the number decimal places.
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Gets or sets the number of rows per page (for pagination).
    /// </summary>
    public int RowsPerPage { get; set; } = 50;
}
