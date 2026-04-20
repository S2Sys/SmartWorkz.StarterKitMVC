namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Sealed implementation of IPdfExporter for exporting data to PDF format using QuestPDF.
/// </summary>
public sealed class PdfExporter : IPdfExporter
{
    private readonly PdfOptions _options;

    /// <summary>
    /// Initializes a new instance of the PdfExporter class.
    /// </summary>
    /// <param name="options">Configuration options for PDF export. If null, default options are used.</param>
    public PdfExporter(PdfOptions? options = null)
    {
        _options = options ?? new PdfOptions();
    }

    /// <summary>
    /// Exports enumerable data to a PDF document with table layout using QuestPDF.
    /// Note: PDF export requires QuestPDF library upgrade to match current API.
    /// </summary>
    public async Task<Result<byte[]>> ExportAsync<T>(IEnumerable<T> data, string title, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var dataList = data?.ToList() ?? new List<T>();

                if (dataList.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoDataToExport", "No data to export.");
                }

                return Result<byte[]>.Fail<byte[]>("Error.FeatureNotImplemented", "PDF export requires implementation update for current QuestPDF version.");
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Fail<byte[]>("Error.PdfExportFailed", $"PDF export failed: {ex.Message}");
            }
        }, ct);
    }

    /// <summary>
    /// Formats the cell value for display in the PDF.
    /// </summary>
    private string FormatCellValue(object? value, System.Reflection.PropertyInfo property)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var type = value.GetType();
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
        {
            return ((DateTime)value).ToString(_options.DateFormat);
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            var propertyName = property.Name.ToLower();
            if (propertyName.Contains("price") || propertyName.Contains("amount") || propertyName.Contains("currency"))
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString(_options.CurrencyFormat);
            }
            else
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString($"F{_options.DecimalPlaces}");
            }
        }
        else if (underlyingType == typeof(bool))
        {
            return (bool)value ? "Yes" : "No";
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the appropriate horizontal alignment for a cell based on its value type.
    /// </summary>
    private string GetCellAlignment(object? value)
    {
        if (value == null)
        {
            return "left";
        }

        var type = value.GetType();
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime) ||
            underlyingType == typeof(decimal) ||
            underlyingType == typeof(double) ||
            underlyingType == typeof(float) ||
            underlyingType == typeof(int) ||
            underlyingType == typeof(long) ||
            underlyingType == typeof(short))
        {
            return "right";
        }

        return "left";
    }

    /// <summary>
    /// Gets the QuestPDF page size based on the configured page size string.
    /// </summary>
    private dynamic GetPageSize()
    {
        var pageSize = _options.PageSize.ToUpper();
        var isLandscape = _options.Orientation.ToLower() == "landscape";

        var size = pageSize switch
        {
            "A4" => PageSizes.A4,
            "LETTER" => PageSizes.Letter,
            "A3" => PageSizes.A3,
            "A5" => PageSizes.A5,
            "LEGAL" => PageSizes.Legal,
            _ => PageSizes.A4
        };

        if (isLandscape)
        {
            size = size.Landscape();
        }

        return size;
    }
}
