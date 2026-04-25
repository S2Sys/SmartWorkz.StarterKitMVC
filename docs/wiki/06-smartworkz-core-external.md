# SmartWorkz.Core.External — PDF & Excel Export

## Assembly Reference

**ProjectReference:** `src/SmartWorkz.Core.External/`  
**Namespace:** `SmartWorkz.Core.External`  
**Target Framework:** .NET 9  
**Key Dependencies:**
- `ClosedXML` v0.101.0 — Excel (XLSX) generation
- `iText7` v7.2.4 — PDF generation (production: use QuestPDF v2024.12.2)

---

## Excel Export — IExcelExporter

### Signature

```csharp
public interface IExcelExporter
{
    Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        CancellationToken cancellationToken = default);
}
```

Returns `Result<byte[]>` containing the XLSX file bytes or error details.

### ExcelOptions — Styling & Layout

Configure Excel generation:

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| `IncludeHeaders` | bool | true | Include column headers in row 1 |
| `HeaderBold` | bool | true | Make header row bold |
| `HeaderFontSize` | int | 12 | Header font size |
| `DataFontSize` | int | 11 | Data font size |
| `AlternateRowColor` | bool | true | Stripe rows (light gray/white) |
| `AutoFitColumns` | bool | true | Auto-size columns to content |
| `DateFormat` | string | "yyyy-MM-dd" | Date display format |
| `CurrencyFormat` | string | "$#,##0.00" | Currency format (USD) |
| `NumberFormat` | string | "#,##0.00" | Decimal number format |
| `FreezeHeaderRow` | bool | true | Keep header visible when scrolling |
| `ApplyAutoFilter` | bool | true | Enable filter dropdowns on headers |

### Usage — Razor Page

```csharp
public class ProductListModel : BaseListPage<ProductDto>
{
    private readonly IExcelExporter _exporter;
    
    public ProductListModel(
        IDapperRepository<Product> repository,
        IExcelExporter exporter) : base(repository)
    {
        _exporter = exporter;
    }
    
    public async Task<IActionResult> OnGetExportAsync()
    {
        // Load data (with filter/sort applied)
        var products = await _repository.GetAllAsync();
        
        // Export
        var result = await _exporter.ExportAsync(
            products,
            "Products",
            HttpContext.RequestAborted);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }
        
        // Return as file download
        return File(
            result.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"products-{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
```

### DI Registration

```csharp
services.AddScoped<IExcelExporter, ExcelExporter>();
```

### Performance Notes

- **Handles 100K+ rows** without memory issues (streaming write)
- **Concurrent exports** are safe (each instance is independent)
- **Large datasets** (1M+ rows) benefit from pagination: export 100K at a time

---

## PDF Export — IPdfExporter

### Signature

```csharp
public interface IPdfExporter
{
    Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string documentTitle,
        CancellationToken cancellationToken = default);
}
```

### PdfOptions — Page Layout

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| `PageSize` | PageSizeEnum | `A4` | A3, A4, A5, Letter, Legal |
| `Orientation` | string | "Portrait" | Portrait or Landscape |
| `TopMargin` | float | 20 | mm |
| `BottomMargin` | float | 20 | mm |
| `LeftMargin` | float | 15 | mm |
| `RightMargin` | float | 15 | mm |
| `IncludePageNumbers` | bool | true | Footer: "Page X of Y" |
| `RowsPerPage` | int | 30 | Max rows before page break |
| `DateFormat` | string | "yyyy-MM-dd" | Date display |
| `FontName` | string | "Helvetica" | Font family |
| `FontSizeHeader` | int | 14 | Header font size |
| `FontSizeData` | int | 11 | Data font size |

### Usage Example

```csharp
public async Task<IActionResult> OnGetPdfAsync()
{
    var result = await _pdfExporter.ExportAsync(
        items: _items,
        documentTitle: "Monthly Report",
        cancellationToken: HttpContext.RequestAborted);
    
    return File(
        result.Data,
        "application/pdf",
        "report.pdf");
}
```

### Current Status

**PDF exporter is a stub** — `IPdfExporter` interface exists, but implementation returns `"Feature not implemented"`. Plan to implement with QuestPDF (flexible, modern) or iText 7 (mature, feature-rich).

---

## Result<T> — Unified Return Type

Both exporters return `Result<T>`:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }      // "EXPORT_FAILED", etc.
    public string? ErrorMessage { get; set; }   // User-friendly message
}
```

**Error Codes:**
- `EXPORT_FAILED` — General export error
- `INVALID_DATA` — Data null or empty
- `EXPORT_TIMEOUT` — Exceeded timeout
- `FILE_WRITE_ERROR` — Disk write failed

---

## Complete Razor Page Example

```csharp
@page "/reports/export"
@model ReportExportModel

<div class="container mt-4">
    <h1>Export Report</h1>
    
    <form method="post" class="mb-4">
        <div class="row">
            <div class="col-md-4">
                <label class="form-label">Format:</label>
                <select name="format" class="form-select">
                    <option value="excel">Excel (XLSX)</option>
                    <option value="pdf">PDF</option>
                </select>
            </div>
            <div class="col-md-4">
                <label class="form-label">Date Range:</label>
                <input type="date" name="fromDate" class="form-control" />
            </div>
            <div class="col-md-4 d-flex align-items-end">
                <button type="submit" class="btn btn-primary w-100">Export</button>
            </div>
        </div>
    </form>
</div>

@code {
    public class ReportExportModel : PageModel
    {
        private readonly IExcelExporter _excelExporter;
        private readonly IPdfExporter _pdfExporter;
        private readonly IReportRepository _repository;
        
        public ReportExportModel(IExcelExporter excelExporter, IPdfExporter pdfExporter, IReportRepository repository)
        {
            _excelExporter = excelExporter;
            _pdfExporter = pdfExporter;
            _repository = repository;
        }
        
        public async Task<IActionResult> OnPostAsync(string format, DateTime fromDate)
        {
            var reports = await _repository.GetReportsSinceAsync(fromDate);
            
            if (format == "excel")
            {
                var result = await _excelExporter.ExportAsync(reports, "Reports");
                return result.IsSuccess
                    ? File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reports.xlsx")
                    : BadRequest(result.ErrorMessage);
            }
            else
            {
                var result = await _pdfExporter.ExportAsync(reports, "Reports");
                return result.IsSuccess
                    ? File(result.Data, "application/pdf", "reports.pdf")
                    : BadRequest(result.ErrorMessage);
            }
        }
    }
}
```

---

**Next:** [SmartWorkz.Mobile — MAUI Components & Services](07-smartworkz-mobile.md)
