# SmartWorkz.Core.External

External integrations and export services including Excel and PDF document generation.

## Getting Started

### Prerequisites
- .NET 9.0 or higher
- SmartWorkz.Core.Shared dependency

### Installation

```xml
<ProjectReference Include="path/to/SmartWorkz.Core.External/SmartWorkz.Core.External.csproj" />
```

Register exporters in dependency injection:

```csharp
services.AddScoped<IExcelExporter, ExcelExporter>();
services.AddScoped<IPdfExporter, PdfExporter>();
```

### Basic Usage

```csharp
using SmartWorkz.Core.External.Export;

// Excel export
var exporter = new ExcelExporter();
var result = await exporter.ExportAsync(data, "Report", ct);

// PDF export (requires implementation update)
var pdfExporter = new PdfExporter(options);
var pdfResult = await pdfExporter.ExportAsync(data, "Report", ct);
```

## Project Structure

- **Export/** — Document export implementations
  - `ExcelExporter` — Excel workbook generation
  - `PdfExporter` — PDF document generation (in progress)
  - `PdfOptions` — PDF configuration options
- **Interfaces/** — Export service contracts
- **Extensions/** — Export extension methods

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| ClosedXML | 0.101.0 | Excel file generation |
| QuestPDF | 2024.12.2 | PDF generation |

## Configuration

### Excel Exporter

```csharp
var options = new ExcelOptions
{
    IncludeHeaders = true,
    HeaderBold = true,
    DateFormat = "yyyy-MM-dd",
    CurrencyFormat = "$#,##0.00"
};

var exporter = new ExcelExporter(options);
var result = await exporter.ExportAsync(data, "MyReport", ct);
```

### PDF Exporter (Planned)

```csharp
var pdfOptions = new PdfOptions
{
    PageSize = "A4",
    Orientation = "Portrait",
    TopMargin = 1,
    LeftMargin = 1,
    IncludePageNumbers = true,
    RowsPerPage = 50
};

var pdfExporter = new PdfExporter(pdfOptions);
// Currently returns: "Feature not implemented"
```

## Excel Export Features

### Basic Export

```csharp
var employees = new[]
{
    new { Name = "John Doe", Department = "IT", Salary = 75000 },
    new { Name = "Jane Smith", Department = "HR", Salary = 65000 }
};

var exporter = new ExcelExporter();
var result = await exporter.ExportAsync(employees, "Employees", ct);

if (result.IsSuccess)
{
    System.IO.File.WriteAllBytes("export.xlsx", result.Data);
}
```

### Formatting Options

```csharp
var options = new ExcelOptions
{
    IncludeHeaders = true,
    HeaderBold = true,
    HeaderFontSize = 12,
    DataFontSize = 10,
    AlternateRowColor = true,
    AutoFitColumns = true,
    DateFormat = "MM/dd/yyyy",
    CurrencyFormat = "$#,##0.00",
    NumberFormat = "#,##0.##"
};
```

## PDF Export Features (Planned Implementation)

Current status: **Feature stub** — requires QuestPDF API update

```csharp
var pdfOptions = new PdfOptions
{
    PageSize = "A4",           // A3, A4, A5, Letter, Legal
    Orientation = "Portrait",  // Portrait, Landscape
    TopMargin = 1.0,          // cm
    BottomMargin = 1.0,
    LeftMargin = 1.0,
    RightMargin = 1.0,
    IncludePageNumbers = true,
    RowsPerPage = 50,
    DateFormat = "yyyy-MM-dd"
};
```

## Return Values

Both exporters return `Result<byte[]>`:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Success Example

```csharp
var result = await exporter.ExportAsync(data, "Report", ct);
if (result.IsSuccess)
{
    // result.Data contains the file bytes
    return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}
```

### Error Handling

```csharp
var result = await exporter.ExportAsync(data, "Report", ct);
if (!result.IsSuccess)
{
    _logger.LogError($"{result.ErrorCode}: {result.ErrorMessage}");
    return BadRequest(result.ErrorMessage);
}
```

## Performance Notes

- **Large Datasets**: ClosedXML handles 100K+ rows efficiently
- **Memory Usage**: Files are built in-memory; stream to disk for very large exports
- **Formatting**: Complex formatting increases generation time

## Known Limitations

- **PDF Export**: Requires QuestPDF API implementation update
- **Excel Max Rows**: Practical limit ~1M rows depending on memory

## Testing

```csharp
[Test]
public async Task ExcelExporter_ExportsData()
{
    var data = new[] { new { Name = "Test", Value = 100 } };
    var exporter = new ExcelExporter();
    var result = await exporter.ExportAsync(data, "Test");
    
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.Data);
}
```

## Future Enhancements

- [ ] PDF export implementation (QuestPDF API)
- [ ] CSV export
- [ ] JSON export
- [ ] Custom column mapping
- [ ] Conditional cell formatting
- [ ] Excel formulas and calculations

## Contributing

Report issues with export formatting or file generation on the project repository.
