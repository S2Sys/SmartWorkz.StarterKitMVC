using System.Collections.Concurrent;
using System.Reflection;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Event handler for adding page numbers to PDF pages.
/// </summary>
internal class PageNumberEventHandler : iText.Kernel.Events.IEventHandler
{
    private readonly float _bottomMargin;

    public PageNumberEventHandler(float bottomMargin)
    {
        _bottomMargin = bottomMargin;
    }

    public void HandleEvent(iText.Kernel.Events.Event @event)
    {
        var docEvent = (iText.Kernel.Events.PdfDocumentEvent)@event;
        var pdf = docEvent.GetDocument();
        var page = docEvent.GetPage();
        var pageSize = page.GetPageSize();
        var pageNumber = pdf.GetPageNumber(page);

        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
        var font = iText.Kernel.Font.PdfFontFactory.CreateFont();

        var pageText = $"Page {pageNumber}";
        var textWidth = 50f; // Approximate width for page number text
        var centerX = pageSize.GetWidth() / 2 - textWidth / 2;
        var bottomY = _bottomMargin / 2;

        canvas.BeginText()
            .MoveText(centerX, bottomY)
            .SetFontAndSize(font, 9)
            .ShowText(pageText)
            .EndText();
    }
}

/// <summary>
/// Sealed implementation of IPdfExporter for exporting data to PDF format using iText 7.
/// Provides free, open-source PDF export without platform limitations or font configuration requirements.
/// </summary>
public sealed class PdfExporter : IPdfExporter
{
    private readonly PdfOptions _options;
    private readonly ILogger<PdfExporter>? _logger;
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
    /// Exports enumerable data to a PDF document with table layout using iText 7.
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
                    if (genEx.StackTrace != null)
                        errMsg += $" | Stack: {genEx.StackTrace}";
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
    /// Generates a PDF document from the provided data using iText 7.
    /// </summary>
    private byte[] GeneratePdf<T>(List<T> dataList, List<PropertyInfo> properties, string title)
    {
        using var memoryStream = new MemoryStream();

        var (pageWidth, pageHeight) = GetPageDimensions();
        var pageSize = new iText.Kernel.Geom.PageSize(pageWidth, pageHeight);

        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);

        // Add event handler for page numbers before creating the document
        if (_options.IncludePageNumbers)
        {
            pdf.AddEventHandler(iText.Kernel.Events.PdfDocumentEvent.END_PAGE, new PageNumberEventHandler((float)_options.BottomMargin));
        }

        var document = new Document(pdf, pageSize);

        // Set margins
        document.SetMargins(
            (float)_options.TopMargin,
            (float)_options.RightMargin,
            (float)_options.BottomMargin,
            (float)_options.LeftMargin);

        // Add title if provided
        if (!string.IsNullOrEmpty(title))
        {
            var titleParagraph = new Paragraph(title)
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10);
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
                    .SetTextAlignment(GetCellTextAlignment(value));
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
    private (float Width, float Height) GetPageDimensions()
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
    /// Gets the appropriate text alignment for a cell based on its value type.
    /// </summary>
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
