namespace SmartWorkz.Core.External.Export;

using System.Collections.Concurrent;
using System.Reflection;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

/// <summary>
/// Sealed implementation of IPdfExportService for exporting data to PDF format using iText7.
/// Provides professional table-based PDF export with comprehensive formatting support.
/// </summary>
public sealed class PdfExportService : IPdfExportService
{
    private readonly ILogger<PdfExportService>? _logger;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    // iText page dimensions (in points)
    private static readonly Dictionary<string, (float Width, float Height)> PageSizes = new()
    {
        { "A4", (595.28f, 841.89f) },
        { "LETTER", (612f, 792f) },
        { "A3", (841.89f, 1190.55f) },
        { "A5", (419.53f, 595.28f) },
        { "LEGAL", (612f, 1008f) }
    };

    /// <summary>
    /// Initializes a new instance of the PdfExportService class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public PdfExportService(ILogger<PdfExportService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets cached property metadata for a type to avoid repeated reflection calls.
    /// </summary>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>Array of public readable properties.</returns>
    private PropertyInfo[] GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead)
             .ToArray());
    }

    /// <summary>
    /// Exports enumerable data to a PDF document with professional table layout.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="title">The title of the PDF document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the PDF file bytes.</returns>
    public async Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string title,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dataList = data?.ToList() ?? new List<T>();

                if (dataList.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoDataToExport", "No data to export.");
                }

                var properties = GetCachedProperties(typeof(T)).ToList();

                if (properties.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoPropertiesFound", "The data type has no public readable properties.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var pdfBytes = GeneratePdf(dataList, properties, title ?? "Report");
                    return Result<byte[]>.Ok(pdfBytes);
                }
                catch (Exception genEx)
                {
                    _logger?.LogError(genEx, "Error generating PDF");
                    var errMsg = $"PDF generation error: {genEx.Message}";
                    if (genEx.InnerException != null)
                        errMsg += $" | Inner: {genEx.InnerException.Message}";
                    return Result<byte[]>.Fail<byte[]>("Error.PdfGenerationFailed", errMsg);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting PDF");
                return Result<byte[]>.Fail<byte[]>("Error.PdfExportFailed", $"PDF export failed: {ex.Message}");
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Generates a PDF document from the provided data using iText7.
    /// </summary>
    /// <typeparam name="T">The type of data items.</typeparam>
    /// <param name="dataList">The list of data to export.</param>
    /// <param name="properties">The properties to include in the export.</param>
    /// <param name="title">The title of the PDF document.</param>
    /// <returns>Byte array containing the PDF document.</returns>
    private byte[] GeneratePdf<T>(List<T> dataList, List<PropertyInfo> properties, string title)
    {
        using var memoryStream = new MemoryStream();

        var (pageWidth, pageHeight) = GetPageDimensions("A4", "portrait");
        var pageSize = new iText.Kernel.Geom.PageSize(pageWidth, pageHeight);

        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);

        var document = new Document(pdf, pageSize);

        // Set margins (top, right, bottom, left)
        document.SetMargins(36, 36, 36, 36);

        // Add title if provided
        if (!string.IsNullOrEmpty(title))
        {
            var titleParagraph = new Paragraph(title)
                .SetFontSize(16)
                .SetBold()
                .SetMarginBottom(12);
            document.Add(titleParagraph);
        }

        // Create table with header and data rows
        var table = new Table(properties.Count);
        table.SetWidth(UnitValue.CreatePercentValue(100));

        // Add header row
        foreach (var prop in properties)
        {
            var headerCell = new Cell()
                .Add(new Paragraph(prop.Name))
                .SetBackgroundColor(new iText.Kernel.Colors.DeviceGray(0.85f))
                .SetPadding(8)
                .SetBold();
            table.AddHeaderCell(headerCell);
        }

        // Add data rows
        foreach (var row in dataList)
        {
            foreach (var prop in properties)
            {
                var value = prop.GetValue(row);
                var formattedValue = FormatCellValue(value, prop);
                var cell = new Cell()
                    .Add(new Paragraph(formattedValue))
                    .SetTextAlignment(GetCellTextAlignment(value))
                    .SetPadding(6);
                table.AddCell(cell);
            }
        }

        document.Add(table);
        document.Close();

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Gets page dimensions based on configured page size and orientation.
    /// </summary>
    /// <param name="pageSize">The page size (A4, Letter, A3, A5, Legal).</param>
    /// <param name="orientation">The page orientation (portrait or landscape).</param>
    /// <returns>A tuple of page width and height in points.</returns>
    private (float Width, float Height) GetPageDimensions(string pageSize, string orientation)
    {
        var size = pageSize.ToUpper();
        var isLandscape = orientation.ToLower() == "landscape";

        if (!PageSizes.TryGetValue(size, out var dimensions))
        {
            dimensions = PageSizes["A4"];
        }

        var (width, height) = dimensions;
        if (isLandscape)
        {
            (width, height) = (height, width);
        }

        return (width, height);
    }

    /// <summary>
    /// Formats the cell value for display in the PDF.
    /// Handles dates, currency, decimals, booleans, and text.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="property">The property metadata.</param>
    /// <returns>Formatted string representation of the value.</returns>
    private string FormatCellValue(object? value, PropertyInfo property)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var type = value.GetType();
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime))
        {
            return ((DateTime)value).ToString("yyyy-MM-dd HH:mm");
        }
        else if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            var propertyName = property.Name.ToLower();
            if (propertyName.Contains("price") || propertyName.Contains("amount") || propertyName.Contains("currency"))
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString("C2");
            }
            else if (propertyName.Contains("percent") || propertyName.Contains("rate"))
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString("P2");
            }
            else
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString("F2");
            }
        }
        else if (underlyingType == typeof(bool))
        {
            return (bool)value ? "Yes" : "No";
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the appropriate text alignment for a cell based on its value type.
    /// Numeric and date values are right-aligned; text is left-aligned.
    /// </summary>
    /// <param name="value">The cell value.</param>
    /// <returns>The text alignment to use.</returns>
    private TextAlignment GetCellTextAlignment(object? value)
    {
        if (value == null)
        {
            return TextAlignment.LEFT;
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
            return TextAlignment.RIGHT;
        }

        return TextAlignment.LEFT;
    }
}
