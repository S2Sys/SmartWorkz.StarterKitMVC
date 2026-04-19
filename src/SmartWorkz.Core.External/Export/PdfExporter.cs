namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Sealed implementation of IPdfExporter for exporting data to PDF format using iTextSharp.
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
    /// Exports enumerable data to a PDF document with table layout.
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

                var firstItem = dataList.FirstOrDefault();
                if (firstItem == null)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.InvalidDataFormat", "Invalid data format.");
                }

                var properties = firstItem.GetType().GetProperties();
                if (properties.Length == 0)
                {
                    return Result<byte[]>.Fail<byte[]>("Error.NoPropertiesToExport", "Data has no properties to export.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    var pageSize = GetPageSize();
                    var document = new Document(pageSize, _options.LeftMargin, _options.RightMargin, _options.TopMargin, _options.BottomMargin);
                    var writer = PdfWriter.GetInstance(document, memoryStream);

                    // Add page event handler for page numbers
                    if (_options.IncludePageNumbers)
                    {
                        writer.PageEvent = new PageNumberEventHandler(_options);
                    }

                    document.Open();

                    // Add title if provided
                    if (!string.IsNullOrEmpty(title))
                    {
                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                        var titleParagraph = new Paragraph(title, titleFont)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 10
                        };
                        document.Add(titleParagraph);

                        var dateParagraph = new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        {
                            Alignment = Element.ALIGN_RIGHT,
                            SpacingAfter = 10
                        };
                        dateParagraph.Font.Size = 9;
                        document.Add(dateParagraph);
                    }

                    // Create and populate table
                    CreateAndPopulateTable(document, properties, dataList.Cast<object>().ToList());

                    document.Close();
                    writer.Close();

                    return Result<byte[]>.Ok(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Fail<byte[]>("Error.PdfExportFailed", $"PDF export failed: {ex.Message}");
            }
        }, ct);
    }

    /// <summary>
    /// Creates and populates the PDF table with data.
    /// </summary>
    private void CreateAndPopulateTable(Document document, System.Reflection.PropertyInfo[] properties, List<object> data)
    {
        var table = new PdfPTable(properties.Length)
        {
            WidthPercentage = 100,
            SpacingBefore = 10,
            SpacingAfter = 10
        };

        // Set column widths proportionally
        var columnWidths = new float[properties.Length];
        for (int i = 0; i < properties.Length; i++)
        {
            columnWidths[i] = 100f / properties.Length;
        }
        table.SetWidths(columnWidths);

        // Add headers
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);

        foreach (var property in properties)
        {
            var headerCell = new PdfPCell(new Phrase(property.Name, headerFont))
            {
                BackgroundColor = _options.HeaderBackgroundColor ?? new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                BorderWidth = 1
            };
            table.AddCell(headerCell);
        }

        // Add data rows
        var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        int rowCount = 0;

        foreach (var item in data)
        {
            if (item == null)
            {
                continue;
            }

            // Add page break if needed
            if (_options.RowsPerPage > 0 && rowCount > 0 && rowCount % _options.RowsPerPage == 0)
            {
                document.Add(table);
                document.NewPage();

                // Recreate table for new page
                table = new PdfPTable(properties.Length)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                table.SetWidths(columnWidths);

                // Re-add headers
                foreach (var property in properties)
                {
                    var headerCell = new PdfPCell(new Phrase(property.Name, headerFont))
                    {
                        BackgroundColor = _options.HeaderBackgroundColor ?? new BaseColor(211, 211, 211),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 8,
                        BorderWidth = 1
                    };
                    table.AddCell(headerCell);
                }
            }

            // Add data cells
            foreach (var property in properties)
            {
                var value = property.GetValue(item);
                var cellText = FormatCellValue(value, property);

                var dataCell = new PdfPCell(new Phrase(cellText, dataFont))
                {
                    HorizontalAlignment = GetCellAlignment(value),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 6,
                    BorderWidth = 1
                };
                table.AddCell(dataCell);
            }

            rowCount++;
        }

        document.Add(table);
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
    private int GetCellAlignment(object? value)
    {
        if (value == null)
        {
            return Element.ALIGN_LEFT;
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
            return Element.ALIGN_RIGHT;
        }

        return Element.ALIGN_LEFT;
    }

    /// <summary>
    /// Gets the iTextSharp page size based on the configured page size string.
    /// </summary>
    private Rectangle GetPageSize()
    {
        var pageSize = _options.PageSize.ToUpper();
        var isLandscape = _options.Orientation.ToLower() == "landscape";

        var rectangle = pageSize switch
        {
            "A4" => PageSize.A4,
            "LETTER" => PageSize.LETTER,
            "A3" => PageSize.A3,
            "A5" => PageSize.A5,
            "LEGAL" => PageSize.LEGAL,
            _ => PageSize.A4
        };

        if (isLandscape)
        {
            rectangle = rectangle.Rotate();
        }

        return rectangle;
    }

    /// <summary>
    /// Helper class for adding page numbers to PDF pages.
    /// </summary>
    private sealed class PageNumberEventHandler : PdfPageEventHelper
    {
        private readonly PdfOptions _options;

        public PageNumberEventHandler(PdfOptions options)
        {
            _options = options;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var pageSize = document.PageSize;
            var pdfContentByte = writer.DirectContent;

            pdfContentByte.BeginText();
            var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            pdfContentByte.SetFontAndSize(font, 9);

            var pageNumber = writer.PageNumber;
            var text = $"Page {pageNumber}";

            pdfContentByte.ShowTextAligned(
                Element.ALIGN_RIGHT,
                text,
                pageSize.GetRight(36),
                pageSize.GetBottom(36),
                0);

            pdfContentByte.EndText();
        }
    }
}
