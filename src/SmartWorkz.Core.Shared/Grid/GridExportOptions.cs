namespace SmartWorkz.Shared;

/// <summary>
/// Configuration for grid data export (CSV, Excel).
/// </summary>
public class GridExportOptions
{
    /// <summary>Export format: "csv" or "excel".</summary>
    public string Format { get; set; } = "csv";

    /// <summary>Whether to export only selected rows (if false, export all filtered data).</summary>
    public bool SelectedRowsOnly { get; set; } = false;

    /// <summary>Column property names to include. Null means all visible columns.</summary>
    public List<string>? IncludeColumns { get; set; }

    /// <summary>Column property names to exclude.</summary>
    public List<string>? ExcludeColumns { get; set; }

    /// <summary>File name without extension (extension added based on Format).</summary>
    public string FileName { get; set; } = "export";

    /// <summary>Whether to include column headers in the export.</summary>
    public bool IncludeHeaders { get; set; } = true;
}
