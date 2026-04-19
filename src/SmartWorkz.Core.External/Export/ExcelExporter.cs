namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Sealed implementation of IExcelExporter for exporting data to Excel format using ClosedXML.
/// </summary>
public sealed class ExcelExporter : IExcelExporter
{
    private readonly ExcelOptions _options;

    /// <summary>
    /// Initializes a new instance of the ExcelExporter class.
    /// </summary>
    /// <param name="options">Configuration options for Excel export. If null, default options are used.</param>
    public ExcelExporter(ExcelOptions? options = null)
    {
        _options = options ?? new ExcelOptions();
    }

    /// <summary>
    /// Exports enumerable data to a single Excel sheet.
    /// </summary>
    public async Task<Result<byte[]>> ExportAsync<T>(IEnumerable<T> data, string sheetName, CancellationToken ct = default)
    {
        try
        {
            var sheets = new Dictionary<string, IEnumerable<object>>
            {
                { sheetName, data.Cast<object>().ToList() }
            };

            return await ExportMultipleAsync(sheets, ct);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Fail<byte[]>("Error.ExcelExportFailed", $"Excel export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports multiple collections to multiple sheets in a single workbook.
    /// </summary>
    public async Task<Result<byte[]>> ExportMultipleAsync(Dictionary<string, IEnumerable<object>> sheets, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (sheets == null || sheets.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoSheetsToExport", "No sheets to export.");
                }

                using (var workbook = new XLWorkbook())
                {
                    int sheetIndex = 0;

                    foreach (var sheet in sheets)
                    {
                        ct.ThrowIfCancellationRequested();

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
            catch (Exception ex)
            {
                return Result<byte[]>.Fail<byte[]>("Error.ExcelExportFailed", $"Excel export failed: {ex.Message}");
            }
        }, ct);
    }

    /// <summary>
    /// Populates a worksheet with data from a collection.
    /// </summary>
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

            // Apply header styling
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
                    // Use SetValue with object type to avoid type conversion issues
                    cell.Value = value.ToString();
                    ApplyDataFormatting(cell, value, property.Name);
                }

                // Apply borders
                ApplyBorders(cell);
            }
        }

        // Auto-fit columns if enabled
        if (_options.AutoColumnWidth)
        {
            worksheet.Columns().AdjustToContents();
        }

        // Freeze panes if enabled
        if (_options.FreezePanes)
        {
            worksheet.SheetView.FreezeRows(1);
        }
    }

    /// <summary>
    /// Applies header styling to a cell.
    /// </summary>
    private void ApplyHeaderStyle(IXLCell cell)
    {
        if (_options.HeaderBold)
        {
            cell.Style.Font.Bold = true;
        }

        cell.Style.Font.FontSize = _options.HeaderFontSize;

        if (!string.IsNullOrEmpty(_options.HeaderBackgroundColor))
        {
            try
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(_options.HeaderBackgroundColor);
            }
            catch
            {
                // If color parsing fails, use default light gray
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("D3D3D3");
            }
        }

        if (_options.CenterHeaders)
        {
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
    }

    /// <summary>
    /// Applies data formatting based on the value type.
    /// </summary>
    private void ApplyDataFormatting(IXLCell cell, object value, string propertyName)
    {
        if (value == null)
        {
            return;
        }

        var type = value.GetType();

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
        {
            cell.Style.DateFormat.Format = _options.DateFormat;
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double))
        {
            // Check if the property name contains "price", "amount", "currency"
            var columnNameLower = propertyName.ToLower();
            if (columnNameLower.Contains("price") || columnNameLower.Contains("amount") || columnNameLower.Contains("currency"))
            {
                cell.Style.NumberFormat.Format = _options.CurrencyFormat;
            }
            else
            {
                // Default number format with specified decimal places
                var format = $"0.{new string('0', _options.DecimalPlaces)}";
                cell.Style.NumberFormat.Format = format;
            }
        }
        else if (underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short))
        {
            cell.Style.NumberFormat.Format = "#,##0";
        }
    }

    /// <summary>
    /// Applies borders to a cell.
    /// </summary>
    private void ApplyBorders(IXLCell cell)
    {
        var borderStyle = _options.BorderStyle.ToLower() == "thick"
            ? XLBorderStyleValues.Thick
            : XLBorderStyleValues.Thin;

        cell.Style.Border.TopBorder = borderStyle;
        cell.Style.Border.BottomBorder = borderStyle;
        cell.Style.Border.LeftBorder = borderStyle;
        cell.Style.Border.RightBorder = borderStyle;
    }
}
