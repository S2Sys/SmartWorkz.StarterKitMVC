using System.Reflection;
using System.Text;
using SmartWorkz.Shared;

namespace SmartWorkz.Web;

/// <summary>
/// Service for exporting grid data to various formats (CSV, Excel).
/// </summary>
public class GridExportService
{
    /// <summary>
    /// Export grid data to CSV format.
    /// </summary>
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
    /// Export grid data to Excel format.
    /// Requires: Install EPPlus NuGet package separately.
    /// </summary>
    public byte[] ExportToExcel<T>(
        List<T> data,
        List<GridColumn> columns,
        GridExportOptions options)
    {
        // Placeholder: Implement with EPPlus when added as dependency
        // For now, return empty array
        return [];
    }

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

