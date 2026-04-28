namespace SmartWorkz.Core.External.Export;

using Microsoft.Extensions.Logging;

/// <summary>
/// Sealed implementation of IExcelExportService for exporting data to Excel format.
/// Uses ClosedXML for Excel file generation with comprehensive formatting support.
/// </summary>
public sealed class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService>? _logger;

    /// <summary>
    /// Initializes a new instance of the ExcelExportService class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public ExcelExportService(ILogger<ExcelExportService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exports enumerable data to a single Excel sheet with automatic formatting.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="sheetName">The name of the Excel sheet.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    public async Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sheets = new Dictionary<string, IEnumerable<object>>
            {
                { string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName, data.Cast<object>().ToList() }
            };

            return await ExportMultipleAsync(sheets, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error exporting Excel");
            return Result<byte[]>.Fail<byte[]>("Error.ExcelExportFailed", $"Excel export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports multiple collections to multiple sheets in a single workbook.
    /// </summary>
    /// <param name="sheets">Dictionary where key is sheet name and value is enumerable data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    public async Task<Result<byte[]>> ExportMultipleAsync(
        Dictionary<string, IEnumerable<object>> sheets,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (sheets == null || sheets.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoSheetsToExport", "No sheets to export.");
                }

                using (var workbook = new XLWorkbook())
                {
                    int sheetIndex = 0;

                    foreach (var sheet in sheets)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var sheetData = sheet.Value?.ToList() ?? new List<object>();
                        if (sheetData.Count == 0)
                        {
                            continue;
                        }

                        var worksheet = workbook.Worksheets.Add(sheet.Key);
                        PopulateSheet(worksheet, sheetData);
                        sheetIndex++;
                    }

                    // Remove default sheet if we have added sheets
                    if (sheetIndex > 0 && workbook.Worksheets.Count > 1)
                    {
                        var defaultSheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Sheet1");
                        if (defaultSheet != null && !sheets.ContainsKey("Sheet1"))
                        {
                            workbook.Worksheets.Delete("Sheet1");
                        }
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(memoryStream);
                        return Result<byte[]>.Ok(memoryStream.ToArray());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting multiple sheets to Excel");
                return Result<byte[]>.Fail<byte[]>("Error.ExcelExportFailed", $"Excel export failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Populates a worksheet with data from a collection, applying automatic formatting.
    /// </summary>
    /// <param name="worksheet">The worksheet to populate.</param>
    /// <param name="data">The data to populate with.</param>
    private void PopulateSheet(IXLWorksheet worksheet, List<object> data)
    {
        if (data.Count == 0)
        {
            return;
        }

        var firstItem = data.FirstOrDefault();
        if (firstItem == null)
        {
            return;
        }

        var properties = firstItem.GetType().GetProperties();
        if (properties.Length == 0)
        {
            return;
        }

        // Add headers
        for (int colIndex = 0; colIndex < properties.Length; colIndex++)
        {
            var cell = worksheet.Cell(1, colIndex + 1);
            cell.Value = properties[colIndex].Name;
            ApplyHeaderStyle(cell);
        }

        // Add data rows
        for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var item = data[rowIndex];
            for (int colIndex = 0; colIndex < properties.Length; colIndex++)
            {
                var property = properties[colIndex];
                var value = property.GetValue(item);
                var cell = worksheet.Cell(rowIndex + 2, colIndex + 1);

                if (value != null)
                {
                    cell.Value = value.ToString();
                    ApplyDataFormatting(cell, value, property.Name);
                }

                ApplyBorders(cell);
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Freeze panes (header row)
        worksheet.SheetView.FreezeRows(1);
    }

    /// <summary>
    /// Applies header styling (bold, gray background, center alignment).
    /// </summary>
    /// <param name="cell">The cell to style.</param>
    private void ApplyHeaderStyle(IXLCell cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 11;
        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("D3D3D3");
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    /// <summary>
    /// Applies data formatting based on the value type (numbers, dates, currency).
    /// </summary>
    /// <param name="cell">The cell to format.</param>
    /// <param name="value">The value to format.</param>
    /// <param name="propertyName">The property name (for context-based formatting).</param>
    private void ApplyDataFormatting(IXLCell cell, object value, string propertyName)
    {
        if (value == null)
        {
            return;
        }

        var type = value.GetType();
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
        {
            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            var columnNameLower = propertyName.ToLower();
            if (columnNameLower.Contains("price") || columnNameLower.Contains("amount") || columnNameLower.Contains("currency"))
            {
                cell.Style.NumberFormat.Format = "$#,##0.00";
            }
            else if (columnNameLower.Contains("percent") || columnNameLower.Contains("rate"))
            {
                cell.Style.NumberFormat.Format = "0.00%";
            }
            else
            {
                cell.Style.NumberFormat.Format = "0.00";
            }
        }
        else if (underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short))
        {
            cell.Style.NumberFormat.Format = "#,##0";
        }
    }

    /// <summary>
    /// Applies borders to a cell for better visual separation.
    /// </summary>
    /// <param name="cell">The cell to apply borders to.</param>
    private void ApplyBorders(IXLCell cell)
    {
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
    }
}
