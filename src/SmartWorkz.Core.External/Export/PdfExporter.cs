using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Sealed implementation of IPdfExporter for exporting data to PDF format using PdfSharp.
/// Provides free, open-source PDF export without platform limitations.
/// </summary>
public sealed class PdfExporter : IPdfExporter
{
    private readonly PdfOptions _options;
    private readonly ILogger<PdfExporter>? _logger;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    // PdfSharp page dimensions (in points)
    private static readonly Dictionary<string, (double Width, double Height)> PageSizes = new()
    {
        { "A4", (595.28, 841.89) },
        { "LETTER", (612, 792) },
        { "A3", (841.89, 1190.55) },
        { "A5", (419.53, 595.28) },
        { "LEGAL", (612, 1008) }
    };

    /// <summary>
    /// Initializes a new instance of the PdfExporter class.
    /// </summary>
    /// <param name="options">Configuration options for PDF export. If null, default options are used.</param>
    /// <param name="logger">Optional logger for error tracking.</param>
    public PdfExporter(PdfOptions? options = null, ILogger<PdfExporter>? logger = null)
    {
        _options = options ?? new PdfOptions();
        _logger = logger;
    }

    /// <summary>
    /// Gets cached property metadata for a type to avoid repeated reflection calls.
    /// </summary>
    private PropertyInfo[] GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead)
             .ToArray());
    }

    /// <summary>
    /// Exports enumerable data to a PDF document with table layout using PdfSharp.
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

                var properties = GetCachedProperties(typeof(T)).ToList();

                if (properties.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoPropertiesFound", "The data type has no public readable properties.");
                }

                byte[] pdfBytes;
                try
                {
                    pdfBytes = GeneratePdf(dataList, properties, title);
                }
                catch (Exception genEx)
                {
                    _logger?.LogError(genEx, "Error generating PDF");
                    var errMsg = $"PDF generation error: {genEx.Message}";
                    if (genEx.InnerException != null)
                        errMsg += $" | Inner: {genEx.InnerException.Message}";
                    return Result<byte[]>.Fail<byte[]>("Error.PdfGenerationFailed", errMsg);
                }

                return Result<byte[]>.Ok(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting PDF");
                return Result<byte[]>.Fail<byte[]>("Error.PdfExportFailed", $"PDF export failed: {ex.Message}");
            }
        }, ct);
    }

    /// <summary>
    /// Generates a PDF document from the provided data.
    /// </summary>
    private byte[] GeneratePdf<T>(List<T> dataList, List<PropertyInfo> properties, string title)
    {
        var document = new PdfDocument();
        var (pageWidth, pageHeight) = GetPageDimensions();

        var rowsPerPage = _options.RowsPerPage;
        var totalPages = (int)Math.Ceiling((double)dataList.Count / rowsPerPage);
        var topMargin = _options.TopMargin;
        var leftMargin = _options.LeftMargin;
        var rightMargin = _options.RightMargin;
        var bottomMargin = _options.BottomMargin;

        var pageNum = 0;
        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            pageNum++;
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);

            var gfx = XGraphics.FromPdfPage(page);
            var yPos = (float)topMargin;

            // Draw title on first page only
            if (pageIndex == 0 && !string.IsNullOrEmpty(title))
            {
                var titleFont = new XFont("Arial", 14);
                gfx.DrawString(title, titleFont, XBrushes.Black,
                    new XRect((float)leftMargin, yPos, (float)(pageWidth - leftMargin - rightMargin), 20),
                    XStringFormats.TopLeft);
                yPos += 25;
            }

            // Draw table header
            var headerFont = new XFont("Arial", 10);
            yPos = DrawTableHeader(gfx, headerFont, properties, pageWidth, leftMargin, rightMargin, yPos);

            // Draw table rows for this page
            var startIndex = pageIndex * rowsPerPage;
            var endIndex = Math.Min(startIndex + rowsPerPage, dataList.Count);

            var dataFont = new XFont("Arial", 9);
            for (int i = startIndex; i < endIndex; i++)
            {
                if (yPos > (pageHeight - bottomMargin - 20))
                {
                    // Move to next page if not enough space
                    break;
                }

                var row = dataList[i];
                yPos = DrawTableRow(gfx, dataFont, row, properties, pageWidth, leftMargin, rightMargin, yPos);
            }

            // Draw page numbers if enabled
            if (_options.IncludePageNumbers)
            {
                var footerFont = new XFont("Arial", 9);
                var pageText = $"Page {pageNum}";
                var textSize = gfx.MeasureString(pageText, footerFont);
                var pageX = (pageWidth - textSize.Width) / 2;
                gfx.DrawString(pageText, footerFont, XBrushes.Black,
                    new XPoint(pageX, pageHeight - bottomMargin + 10));
            }
        }

        // Save to memory stream
        using (var memoryStream = new MemoryStream())
        {
            document.Save(memoryStream, false);
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Draws the table header row.
    /// </summary>
    private float DrawTableHeader(XGraphics gfx, XFont font, List<PropertyInfo> properties,
        double pageWidth, double leftMargin, double rightMargin, float yPos)
    {
        var contentWidth = pageWidth - leftMargin - rightMargin;
        var cellWidth = contentWidth / properties.Count;
        const float rowHeight = 20;

        // Draw header background and text
        var brush = new XSolidBrush(XColor.FromArgb(211, 211, 211));
        for (int i = 0; i < properties.Count; i++)
        {
            var xPos = (float)(leftMargin + (i * cellWidth));

            // Draw header cell background
            gfx.DrawRectangle(brush, xPos, yPos, (float)cellWidth, rowHeight);
            gfx.DrawRectangle(XPens.Black, xPos, yPos, (float)cellWidth, rowHeight);

            // Draw header text
            var textRect = new XRect(xPos + 2, yPos + 2, (float)(cellWidth - 4), rowHeight - 4);
            gfx.DrawString(properties[i].Name, font, XBrushes.Black, textRect, XStringFormats.TopLeft);
        }

        return yPos + rowHeight;
    }

    /// <summary>
    /// Draws a single table data row.
    /// </summary>
    private float DrawTableRow<T>(XGraphics gfx, XFont font, T row, List<PropertyInfo> properties,
        double pageWidth, double leftMargin, double rightMargin, float yPos)
    {
        var contentWidth = pageWidth - leftMargin - rightMargin;
        var cellWidth = contentWidth / properties.Count;
        const float rowHeight = 18;

        for (int i = 0; i < properties.Count; i++)
        {
            var xPos = (float)(leftMargin + (i * cellWidth));
            var prop = properties[i];
            var value = prop.GetValue(row);
            var formattedValue = FormatCellValue(value, prop);
            var alignment = GetCellAlignment(value) == "right" ? XStringFormats.TopRight : XStringFormats.TopLeft;

            // Draw cell border
            gfx.DrawRectangle(XPens.Black, xPos, yPos, (float)cellWidth, rowHeight);

            // Draw cell text
            var textRect = new XRect(xPos + 2, yPos + 2, (float)(cellWidth - 4), rowHeight - 4);
            gfx.DrawString(formattedValue, font, XBrushes.Black, textRect, alignment);
        }

        return yPos + rowHeight;
    }

    /// <summary>
    /// Gets page dimensions based on configured page size and orientation.
    /// </summary>
    private (double Width, double Height) GetPageDimensions()
    {
        var pageSize = _options.PageSize.ToUpper();
        var isLandscape = _options.Orientation.ToLower() == "landscape";

        if (!PageSizes.TryGetValue(pageSize, out var dimensions))
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
    /// </summary>
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
}
