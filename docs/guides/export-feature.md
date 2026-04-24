# Export Feature Guide

The export feature allows users to download data in PDF or Excel format through the API.

## API Endpoint

**POST /api/export**

### Request Body

```json
{
  "data": [
    { "name": "John", "age": 30 },
    { "name": "Jane", "age": 28 }
  ],
  "title": "Employee Report",
  "format": "pdf"
}
```

- `data`: Array of objects to export (required)
- `title`: Report title (optional, defaults to "Export")
- `format`: "pdf" or "excel" (required)

### Response

- **PDF**: `application/pdf` with `.pdf` file
- **Excel**: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` with `.xlsx` file

### Example

```bash
curl -X POST http://localhost:5000/api/export \
  -H "Content-Type: application/json" \
  -d '{
    "data": [{"name":"John","age":30}],
    "title": "Test",
    "format": "pdf"
  }' \
  -o report.pdf
```

## Formatting Options

### PDF Export (PdfOptions)

- `PageSize`: A3, A4, A5, Letter, Legal (default: A4)
- `Orientation`: Portrait or Landscape (default: Portrait)
- `Margins`: Top, Bottom, Left, Right in points
- `HeaderFontBold`: Make header text bold (default: true)
- `HeaderBackgroundColor`: Header background color
- `IncludePageNumbers`: Add page numbers to footer (default: false)
- `DateFormat`: Format for DateTime values (default: "yyyy-MM-dd")
- `CurrencyFormat`: Format for decimal values (default: "C")

### Excel Export (ExcelOptions)

- `HeaderFontBold`: Make header text bold (default: true)
- `HeaderBackgroundColor`: Header background color
- `IncludeTotals`: Add sum row for numeric columns (default: false)
- `DateFormat`: Format for DateTime values (default: "yyyy-MM-dd")

## Usage in Code

```csharp
// Inject the exporters
public MyController(IPdfExporter pdfExporter, IExcelExporter excelExporter)
{
    _pdfExporter = pdfExporter;
    _excelExporter = excelExporter;
}

// Export to PDF
var data = new[] { new { Name = "John", Age = 30 } };
var result = await _pdfExporter.ExportAsync(data, "My Report", CancellationToken.None);

if (result.Succeeded)
{
    // Use result.Data (byte array)
}
```

## Limitations

- Dynamic object properties are discovered via reflection
- Very large datasets (>10,000 rows) may take time
- Special characters are preserved but font support depends on QuestPDF/ClosedXML
- PDF export is currently tested on x86/x64 architectures (QuestPDF does not support ARM64)

## Testing

Unit tests located in:
- `tests/SmartWorkz.Core.External.Tests/Export/PdfExporterTests.cs`
- `tests/SmartWorkz.Core.External.Tests/Export/ExcelExporterTests.cs`

Integration tests:
- `tests/SmartWorkz.StarterKitMVC.Tests.Integration/Endpoints/ExportEndpointTests.cs`

Run: `dotnet test tests/SmartWorkz.Core.External.Tests/Export/ -v`
