using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Sealed implementation of IPdfExporter for exporting data to PDF format using QuestPDF.
/// </summary>
public sealed class PdfExporter : IPdfExporter
{
    private readonly PdfOptions _options;
    private readonly ILogger<PdfExporter>? _logger;

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

                var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToList();

                if (properties.Count == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoPropertiesFound", "The data type has no public readable properties.");
                }

                var pageSize = GetPageSize();

                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    container
                        .Page(page =>
                        {
                            page.Size(pageSize);
                            page.MarginTop(_options.TopMargin);
                            page.MarginRight(_options.RightMargin);
                            page.MarginBottom(_options.BottomMargin);
                            page.MarginLeft(_options.LeftMargin);

                            page.Header().Element(header =>
                            {
                                if (!string.IsNullOrEmpty(title))
                                {
                                    header.Text(title)
                                        .FontSize(14)
                                        .Bold();
                                }
                            });

                            page.Content().Element(content =>
                            {
                                content.Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        foreach (var _ in properties)
                                        {
                                            columns.RelativeColumn(1);
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        var headerColor = (_options.HeaderBackgroundColor as QuestPDF.Infrastructure.Color?) ?? Colors.Grey.Lighten2;
                                        foreach (var property in properties)
                                        {
                                            header.Cell()
                                                .Background(headerColor)
                                                .Padding(5)
                                                .Text(property.Name)
                                                .FontSize(10)
                                                .Bold();
                                        }
                                    });

                                    foreach (var row in dataList)
                                    {
                                        foreach (var property in properties)
                                        {
                                            var value = property.GetValue(row);
                                            var formattedValue = FormatCellValue(value, property);
                                            var alignment = GetCellAlignment(value);

                                            var cell = table.Cell().Padding(5).Text(formattedValue).FontSize(9);
                                            if (alignment == "right")
                                            {
                                                cell.AlignRight();
                                            }
                                        }
                                    }
                                });
                            });

                            if (_options.IncludePageNumbers)
                            {
                                page.Footer().AlignCenter().Text(x =>
                                {
                                    x.Span("Page ");
                                    x.CurrentPageNumber();
                                });
                            }
                        });
                });

                byte[] pdfBytes;
                try
                {
                    pdfBytes = document.GeneratePdf();
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
