using System.Reflection;
using System.Text;
using SmartWorkz.Shared;

namespace SmartWorkz.Web;

/// <summary>
/// Service for exporting grid data to various formats (CSV, Excel).
///
/// GridExportService exports tabular data from grid components to CSV or Excel formats.
/// It handles column filtering, header inclusion, and proper escaping of special characters
/// according to CSV standards (RFC 4180).
/// </summary>
/// <remarks>
/// ## CSV Export Details
/// The ExportToCsv method generates CSV output with the following specifications:
/// - **Column Filtering**: Only visible columns are included unless overridden by IncludeColumns/ExcludeColumns
/// - **Headers**: First row contains column DisplayName values (or empty if IncludeHeaders is false)
/// - **Data Rows**: One row per item, with property values extracted via reflection
/// - **Escaping**: Values containing comma, quote, or newline are wrapped in quotes; internal quotes are doubled
/// - **Delimiter**: Comma (,) separates fields; RFC 4180 compliant
/// - **Encoding**: UTF-8
/// - **Line Endings**: CRLF (Windows-style) for compatibility
///
/// ## Column Selection Logic
/// GetColumnsToExport filters columns in this order:
/// 1. Start with all visible columns (IsVisible == true)
/// 2. If IncludeColumns is specified, further filter to only those columns
/// 3. If ExcludeColumns is specified, remove those columns from the result
///
/// ## CSV Escaping (RFC 4180)
/// - If a value contains comma, quote, or newline: wrap entire value in double quotes
/// - If a value contains a double quote: escape it by doubling (e.g., "Dr. Smith" → "Dr. Smith")
/// - Empty values: output as empty (no quotes) unless they contain special characters
///
/// Example escaping:
/// - Input: John Smith → Output: John Smith (no quotes needed)
/// - Input: Smith, John → Output: "Smith, John" (comma requires quotes)
/// - Input: Dr. "Doc" → Output: "Dr. ""Doc""" (quote is escaped and field is quoted)
/// - Input: Line 1\nLine 2 → Output: "Line 1\nLine 2" (newline requires quotes)
///
/// ## Excel Export (Not Yet Implemented)
/// The ExportToExcel method is currently a placeholder. Future implementation will require:
/// - EPPlus NuGet package (install via: dotnet add package EPPlus)
/// - Dependency injection of EPPlus license (EPPlus 5.0+ requires license for certain operations)
/// - Formatting similar to CSV but with enhanced Excel features (cell styling, formulas, etc.)
/// - Returns byte[] containing Excel file (.xlsx) binary content
///
/// ## Integration with GridComponent
/// Typical usage in a Razor component:
/// ```csharp
/// var service = new GridExportService();
/// var columns = new List&lt;GridColumn&gt; { /* ... */ };
/// var options = new GridExportOptions { Format = "csv", FileName = "products" };
/// var csvContent = service.ExportToCsv(data, columns, options);
/// // Send csvContent to browser for download
/// ```
///
/// ## Use Cases
/// - Download grid data for offline analysis
/// - Backup data before deletion
/// - Integration with external systems
/// - User reports and exports
/// - Excel analysis (when Excel export is implemented)
/// </remarks>
public class GridExportService
{
    /// <summary>
    /// Exports grid data to CSV format with proper escaping and optional column filtering.
    /// </summary>
    /// <remarks>
    /// The CSV output follows RFC 4180 standard with CRLF line endings.
    /// Columns are determined by GetColumnsToExport using IncludeColumns/ExcludeColumns.
    /// Values containing comma, quote, or newline are automatically escaped.
    /// </remarks>
    /// <typeparam name="T">The type of items in the data collection.</typeparam>
    /// <param name="data">The collection of items to export.</param>
    /// <param name="columns">The list of GridColumn definitions (PropertyName, DisplayName, IsVisible, etc).</param>
    /// <param name="options">Export options including Format, IncludeHeaders, IncludeColumns, ExcludeColumns.</param>
    /// <returns>A CSV-formatted string ready for file download or further processing.</returns>
    /// <example>
    /// // Export products with ID, Name, Price columns
    /// var service = new GridExportService();
    /// var data = new List&lt;Product&gt;
    /// {
    ///     new() { Id = 1, Name = "Product A", Price = 99.99m },
    ///     new() { Id = 2, Name = "Product B", Price = 149.99m }
    /// };
    /// var columns = new List&lt;GridColumn&gt;
    /// {
    ///     new() { PropertyName = "Id", DisplayName = "ID" },
    ///     new() { PropertyName = "Name", DisplayName = "Product Name" },
    ///     new() { PropertyName = "Price", DisplayName = "Price" }
    /// };
    /// var options = new GridExportOptions { Format = "csv", IncludeHeaders = true };
    /// var csv = service.ExportToCsv(data, columns, options);
    ///
    /// // Output:
    /// // ID,Product Name,Price
    /// // 1,Product A,99.99
    /// // 2,Product B,149.99
    /// </example>
    public string ExportToCsv<T>(
        List<T> data,
        List<GridColumn> columns,
        GridExportOptions options)
    {
        var sb = new StringBuilder();

        // Get columns to export
        var columnsToExport = GetColumnsToExport(columns, options);

        // Write headers
        if (options.IncludeHeaders)
        {
            var headers = string.Join(",", columnsToExport.Select(c => EscapeCsv(c.DisplayName)));
            sb.AppendLine(headers);
        }

        // Write data rows
        foreach (var item in data)
        {
            var values = new List<string>();
            foreach (var column in columnsToExport)
            {
                var property = typeof(T).GetProperty(column.PropertyName);
                var value = property?.GetValue(item)?.ToString() ?? string.Empty;
                values.Add(EscapeCsv(value));
            }
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports grid data to Excel format. Currently not implemented.
    /// </summary>
    /// <remarks>
    /// **Status**: Placeholder method - returns empty byte array.
    ///
    /// Future implementation will require:
    /// - EPPlus NuGet package installation: `dotnet add package EPPlus`
    /// - EPPlus 5.0+ license activation (required for most operations)
    /// - Cell styling, number formatting, and worksheet customization
    ///
    /// Planned parameters and behavior will match ExportToCsv:
    /// - data: Collection of items to export
    /// - columns: GridColumn definitions with display names and visibility
    /// - options: IncludeColumns, ExcludeColumns, FileName, IncludeHeaders
    ///
    /// The method will return the Excel file (.xlsx) as a byte array suitable for
    /// browser download or file storage.
    /// </remarks>
    /// <typeparam name="T">The type of items in the data collection.</typeparam>
    /// <param name="data">The collection of items to export.</param>
    /// <param name="columns">The list of GridColumn definitions.</param>
    /// <param name="options">Export options (same as CSV).</param>
    /// <returns>Empty byte array. Actual implementation pending EPPlus dependency.</returns>
    public byte[] ExportToExcel<T>(
        List<T> data,
        List<GridColumn> columns,
        GridExportOptions options)
    {
        // Placeholder: Implement with EPPlus when added as dependency
        // For now, return empty array
        return [];
    }

    /// <summary>
    /// Determines which columns to include in the export based on visibility and options.
    /// </summary>
    /// <remarks>
    /// The column filtering logic is applied in this order:
    /// 1. Start with all visible columns (IsVisible == true)
    /// 2. If IncludeColumns is specified, further restrict to only those columns
    /// 3. If ExcludeColumns is specified, remove those columns from the result
    ///
    /// This allows flexible column selection: start with all visible, then optionally
    /// include only certain columns or exclude specific ones.
    /// </remarks>
    /// <param name="columns">The complete list of available columns.</param>
    /// <param name="options">Export options specifying IncludeColumns and ExcludeColumns.</param>
    /// <returns>List of columns that should be included in the export.</returns>
    private static List<GridColumn> GetColumnsToExport(
        List<GridColumn> columns,
        GridExportOptions options)
    {
        var result = columns.Where(c => c.IsVisible).ToList();

        if (options.IncludeColumns?.Any() == true)
        {
            result = result
                .Where(c => options.IncludeColumns.Contains(c.PropertyName))
                .ToList();
        }

        if (options.ExcludeColumns?.Any() == true)
        {
            result = result
                .Where(c => !options.ExcludeColumns.Contains(c.PropertyName))
                .ToList();
        }

        return result;
    }

    /// <summary>
    /// Escapes a value for CSV output according to RFC 4180 standard.
    /// </summary>
    /// <remarks>
    /// CSV escaping rules (RFC 4180):
    /// - If value contains comma, quote ("), or newline: wrap the entire value in double quotes
    /// - Within quoted values, escape double quotes by doubling them: " → ""
    /// - Empty strings: output as-is (no quotes needed)
    ///
    /// Examples:
    /// - "Smith, John" (contains comma) → "Smith, John" (quoted, with comma inside)
    /// - 'Dr. "Doc" Smith' (contains quote) → "Dr. ""Doc"" Smith" (quoted, quote doubled)
    /// - "Line 1\nLine 2" (contains newline) → "Line 1\nLine 2" (quoted)
    /// - "Normal Value" (no special chars) → Normal Value (not quoted)
    /// - "" (empty) → "" (empty string, no quotes)
    /// </remarks>
    /// <param name="value">The raw value to escape. Can be null or empty.</param>
    /// <returns>The escaped value, quoted if necessary, ready for CSV output.</returns>
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

