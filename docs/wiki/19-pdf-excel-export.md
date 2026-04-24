# PDF/Excel Export Feature

## Overview

The PDF/Excel export feature enables exporting collection data to binary file formats with configurable styling, formatting, and layout options. Use this feature when building reports, data downloads, or document generation flows that require both PDF and Excel support. The feature provides two unified exporters (IPdfExporter, IExcelExporter) with fluent API extensions and comprehensive configuration for headers, pagination, currency formatting, and more.

---

## Architecture

### Export Service Architecture

```
┌────────────────────────────────────────────────┐
│      Application Layer (Controller/Page)       │
└────────────────────┬─────────────────────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
   ┌────▼─────┐           ┌──────▼──────┐
   │ Direct   │           │ API         │
   │ Service  │           │ Endpoint    │
   │ Injection│           │ (JSON Body) │
   └────┬─────┘           └──────┬──────┘
        │                        │
        └────────────┬───────────┘
                     │
        ┌────────────▼────────────┐
        │  IPdfExporter           │
        │  IExcelExporter         │
        └────────────┬────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
┌───▼──┐    ┌────────▼─────┐    ┌────▼────┐
│iText7│    │ClosedXML     │    │Reflection
│      │    │              │    │Caching
│PDF   │    │Excel Sheets  │    │
│Gen.  │    │              │    │[Type]→
└──────┘    └──────────────┘    │PropertyInfo[]
                                 └────────────┘

Reflection Caching:
ConcurrentDictionary<Type, PropertyInfo[]>
├─ Type: Product → PropertyInfo[] (Id, Name, Price)
├─ Type: Order → PropertyInfo[] (Id, Date, Total)
└─ Type: Invoice → PropertyInfo[] (Number, Customer, Amount)
```

### Architecture Notes

- **Reflection Caching Optimization**: The exporters use `ConcurrentDictionary<Type, PropertyInfo[]>` to cache property metadata per type. This eliminates repeated reflection overhead when exporting multiple instances of the same type.
- **Result<byte[]> Return Type**: Both exporters return `Result<byte[]>` (not `Task<byte[]>`) for consistency with SmartWorkz CQRS patterns, enabling proper error handling and failure tracking.
- **Two Service Models**: Direct service injection in business logic or API endpoint for HTTP requests with configuration in request body.

---

## Quick Start

Minimal 5-line PDF and Excel export:

```csharp
// Inject exporters
public class OrderController
{
    private readonly IPdfExporter _pdfExporter;
    private readonly IExcelExporter _excelExporter;

    public async Task<IActionResult> ExportOrdersPdf(List<Order> orders)
    {
        var result = await _pdfExporter.ExportAsync(orders, "Orders"); // CancellationToken optional (default: CancellationToken.None)
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return File(result.Value, "application/pdf", "orders.pdf");
    }

    public async Task<IActionResult> ExportOrdersExcel(List<Order> orders)
    {
        var result = await _excelExporter.ExportAsync(orders, "Orders"); // CancellationToken optional (default: CancellationToken.None)
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
    }
}
```

---

## Configuration

### PdfOptions (14 Properties)

Configuration for PDF export styling, layout, and formatting:

| Property | Type | Default | Example |
|----------|------|---------|---------|
| PageSize | string | "A4" | "Letter", "A3", "A5", "Legal" |
| Orientation | string | "Portrait" | "Landscape" |
| Title | string? | null | "Sales Report Q1 2024" |
| IncludePageNumbers | bool | true | false |
| TopMargin | float | 36 | 54 (0.75 inch) |
| BottomMargin | float | 36 | 54 (0.75 inch) |
| LeftMargin | float | 36 | 72 (1 inch) |
| RightMargin | float | 36 | 72 (1 inch) |
| HeaderBold | bool | true | false |
| HeaderBackgroundColor | object? | null | Any format: hex `"#FF0000"`, RGB `Color.Red`, or Color objects |
| DateFormat | string | "yyyy-MM-dd" | "MM/dd/yyyy" |
| CurrencyFormat | string | "C" | "$#,##0.00" |
| DecimalPlaces | int | 2 | 3 |
| RowsPerPage | int | 50 | 100 |

### ExcelOptions (11 Properties)

Configuration for Excel export styling, formatting, and layout:

| Property | Type | Default | Example |
|----------|------|---------|---------|
| SheetName | string | "Sheet1" | "Orders" |
| HeaderBold | bool | true | false |
| HeaderBackgroundColor | string | "D3D3D3" | Hex color only: `"4472C4"` or `"#FF0000"` |
| HeaderFontSize | int | 11 | 12 |
| AutoColumnWidth | bool | true | false |
| FreezePanes | bool | true | false |
| BorderStyle | string | "thin" | "thick" |
| CenterHeaders | bool | true | false |
| DateFormat | string | "yyyy-MM-dd" | "MM/dd/yyyy" |
| CurrencyFormat | string | "$#,##0.00" | "C" |
| DecimalPlaces | int | 2 | 3 |

---

## Usage Examples

### Example 1: Service Layer PDF Export with Result Handling

Inject IPdfExporter and handle Result<byte[]> return type properly:

```csharp
public class OrderReportService
{
    private readonly IPdfExporter _pdfExporter;

    public OrderReportService(IPdfExporter pdfExporter)
    {
        _pdfExporter = pdfExporter;
    }

    public async Task<Result<FileContent>> GenerateOrderPdfAsync(List<Order> orders)
    {
        var options = new PdfOptions
        {
            PageSize = "A4",
            Orientation = "Portrait",
            Title = "Order Report",
            IncludePageNumbers = true,
            DateFormat = "MM/dd/yyyy",
            CurrencyFormat = "$#,##0.00"
        };

        var exporter = new PdfExporter(options);
        var result = await exporter.ExportAsync(orders, "Orders"); // CancellationToken optional (default: CancellationToken.None)

        if (result.IsFailure)
            return Result<FileContent>.Fail(result.Error);

        return Result<FileContent>.Ok(new FileContent
        {
            Bytes = result.Value,
            ContentType = "application/pdf",
            FileName = "orders.pdf"
        });
    }
}
```

### Example 2: Multi-Sheet Excel with ExportMultipleAsync

Export multiple collections to separate sheets in a single workbook:

```csharp
public class ReportController : ControllerBase
{
    private readonly IExcelExporter _excelExporter;

    [HttpPost("export-multi")]
    public async Task<IActionResult> ExportMultipleSheets(
        List<Order> orders,
        List<Customer> customers,
        List<Product> products)
    {
        var options = new ExcelOptions
        {
            HeaderBold = true,
            HeaderBackgroundColor = "4472C4",
            AutoColumnWidth = true,
            FreezePanes = true,
            BorderStyle = "thin"
        };

        var exporter = new ExcelExporter(options);
        
        var sheets = new Dictionary<string, IEnumerable<object>>
        {
            { "Orders", orders.Cast<object>().ToList() },
            { "Customers", customers.Cast<object>().ToList() },
            { "Products", products.Cast<object>().ToList() }
        };

        var result = await exporter.ExportMultipleAsync(sheets); // CancellationToken optional (default: CancellationToken.None)

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "report.xlsx"
        );
    }
}
```

### Example 3: Fluent API with Extension Methods

Use ToPdfAsync and ToExcelAsync extensions for clean, chainable exports:

```csharp
public class DashboardService
{
    public async Task<IActionResult> ExportDashboard(
        List<SalesMetric> metrics,
        ExportFormat format)
    {
        var results = format switch
        {
            ExportFormat.Pdf => await metrics
                .ToPdfAsync("Sales Metrics", new PdfOptions
                {
                    Orientation = "Landscape",
                    PageSize = "A4",
                    DateFormat = "yyyy-MM-dd",
                    CurrencyFormat = "$#,##0.00"
                }),
            
            ExportFormat.Excel => await metrics
                .ToExcelAsync("Metrics", new ExcelOptions
                {
                    SheetName = "Sales Data",
                    HeaderBold = true,
                    AutoColumnWidth = true,
                    DateFormat = "yyyy-MM-dd"
                }),
            
            _ => Result<byte[]>.Fail("Error.UnsupportedFormat", "Format not supported")
        };

        if (results.IsFailure)
            return BadRequest(results.Error);

        var contentType = format == ExportFormat.Pdf
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileName = format == ExportFormat.Pdf ? "metrics.pdf" : "metrics.xlsx";

        return File(results.Value, contentType, fileName);
    }
}

public enum ExportFormat { Pdf, Excel }
```

### Example 4: POST /api/export with JSON Configuration

Export via HTTP with configuration in request body:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IPdfExporter _pdfExporter;
    private readonly IExcelExporter _excelExporter;

    [HttpPost("orders")]
    public async Task<IActionResult> ExportOrders([FromBody] ExportRequest request)
    {
        var orders = await _orderService.GetOrdersAsync(request.Filters);

        var result = request.Format switch
        {
            "pdf" => await _pdfExporter.ExportAsync(
                orders,
                "Orders",
                new PdfOptions
                {
                    Orientation = request.Orientation ?? "Portrait",
                    PageSize = request.PageSize ?? "A4",
                    DateFormat = request.DateFormat ?? "yyyy-MM-dd",
                    CurrencyFormat = request.CurrencyFormat ?? "$#,##0.00"
                }
                // CancellationToken optional (default: CancellationToken.None)
            ),
            
            "excel" => await _excelExporter.ExportAsync(
                orders,
                request.SheetName ?? "Orders",
                new ExcelOptions
                {
                    HeaderBold = request.HeaderBold ?? true,
                    AutoColumnWidth = request.AutoColumnWidth ?? true,
                    FreezePanes = request.FreezePanes ?? true,
                    DateFormat = request.DateFormat ?? "yyyy-MM-dd",
                    CurrencyFormat = request.CurrencyFormat ?? "$#,##0.00"
                }
                // CancellationToken optional (default: CancellationToken.None)
            ),
            
            _ => Result<byte[]>.Fail("Error.UnsupportedFormat", "Only 'pdf' and 'excel' supported")
        };

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var contentType = request.Format == "pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(result.Value, contentType, $"export.{(request.Format == "pdf" ? "pdf" : "xlsx")}");
    }
}

public class ExportRequest
{
    public string Format { get; set; } = "pdf"; // "pdf" or "excel"
    public Dictionary<string, object> Filters { get; set; }
    public string? Orientation { get; set; } // PDF only
    public string? PageSize { get; set; } // PDF only
    public string? SheetName { get; set; } // Excel only
    public string? DateFormat { get; set; }
    public string? CurrencyFormat { get; set; }
    public bool? HeaderBold { get; set; }
    public bool? AutoColumnWidth { get; set; }
    public bool? FreezePanes { get; set; }
}
```

---

## API Reference

### IPdfExporter

**Namespace:** `SmartWorkz.Core.External.Export`

**Methods:**

| Method | Return Type | Parameters | Description |
|--------|-------------|-----------|-------------|
| ExportAsync<T> | Task<Result<byte[]>> | data: IEnumerable<T>, title: string, ct: CancellationToken | Export collection to PDF with page title |

**Usage:**

```csharp
var exporter = new PdfExporter(new PdfOptions { PageSize = "A4" });
var result = await exporter.ExportAsync(orders, "Sales Orders");
```

### IExcelExporter

**Namespace:** `SmartWorkz.Core.External.Export`

**Methods:**

| Method | Return Type | Parameters | Description |
|--------|-------------|-----------|-------------|
| ExportAsync<T> | Task<Result<byte[]>> | data: IEnumerable<T>, sheetName: string, ct: CancellationToken | Export collection to single Excel sheet |
| ExportMultipleAsync | Task<Result<byte[]>> | sheets: Dictionary<string, IEnumerable<object>>, ct: CancellationToken | Export multiple collections to multiple sheets in one workbook |

**Usage:**

```csharp
var exporter = new ExcelExporter(new ExcelOptions { SheetName = "Orders" });
var result = await exporter.ExportAsync(orders, "Orders");

var multiResult = await exporter.ExportMultipleAsync(new Dictionary<string, IEnumerable<object>>
{
    { "Orders", orders.Cast<object>().ToList() },
    { "Customers", customers.Cast<object>().ToList() }
});
```

### ExportExtensions

**Namespace:** `SmartWorkz.Core.External.Export`

**Extension Methods:**

```csharp
// PDF fluent API
public static async Task<Result<byte[]>> ToPdfAsync<T>(
    this IEnumerable<T> data,
    string title,
    PdfOptions? options = null,
    CancellationToken ct = default)

// Excel fluent API
public static async Task<Result<byte[]>> ToExcelAsync<T>(
    this IEnumerable<T> data,
    string sheetName,
    ExcelOptions? options = null,
    CancellationToken ct = default)
```

**Usage:**

```csharp
var pdfResult = await orders.ToPdfAsync("Orders", new PdfOptions { Orientation = "Landscape" });
var excelResult = await orders.ToExcelAsync("Orders", new ExcelOptions { AutoColumnWidth = true });
```

### Result<byte[]> Pattern

Both exporters return `Result<byte[]>` for unified error handling:

```csharp
public sealed record Result<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; init; }
    
    public static Result<T> Ok(T value) => new() { Value = value, IsSuccess = true };
    public static Result<T> Fail(string error) => new() { IsSuccess = false, Error = error };
}
```

---

## Integration Notes

### Cross-References

- **[Result Pattern](./04-result-pattern.md)** — Error handling with Result<T> type
- **[Pagination Factory Method](./07-pagination-factory-method.md)** — How to structure filtered datasets for export
- **[Multi-Tenant Architecture](./11-multi-tenant-architecture.md)** — Exporting per-tenant data with proper filtering
- **[HTMX List Pattern](./05-htmx-list-pattern.md)** — Integration point for export buttons in list views

### Controller Integration Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IPdfExporter _pdfExporter;
    private readonly IExcelExporter _excelExporter;
    private readonly IDataService _dataService;

    [HttpGet("export/{format}")]
    public async Task<IActionResult> Export(
        string format,
        [FromQuery] int? tenantId,
        [FromQuery] string? filterCriteria)
    {
        var data = await _dataService.GetDataAsync(tenantId, filterCriteria);

        var result = format.ToLower() switch
        {
            "pdf" => await _pdfExporter.ExportAsync(data, "Export"),
            "xlsx" => await _excelExporter.ExportAsync(data, "Data"),
            _ => Result<byte[]>.Fail("Unsupported format")
        };

        if (result.IsFailure)
            return BadRequest(result.Error);

        var contentType = format.ToLower() == "pdf" 
            ? "application/pdf" 
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(result.Value, contentType, $"export.{format}");
    }
}
```

### Reflection Caching Behavior

The exporters automatically cache property metadata per type to optimize repeated exports. **You don't need to manage caching yourself** — it's entirely transparent.

**How it works:**
- **First call:** Reflection scans `Type.GetProperties()` (slow, ~1ms per type)
- **Subsequent calls:** Cached `PropertyInfo[]` retrieved from `ConcurrentDictionary<Type, PropertyInfo[]>` (fast, <0.1ms per type)

**You only need to understand this if:**
- You're exporting the same type millions of times (you'll see a performance cliff after the first call)
- You're debugging reflection-related errors

**Example:**
```csharp
// First call: Reflection scans Product.GetProperties() (slow)
var result1 = await exporter.ExportAsync(products1, "Products");

// Subsequent calls: Cached PropertyInfo[] used (fast)
var result2 = await exporter.ExportAsync(products2, "Products");
var result3 = await exporter.ExportAsync(products3, "Products");
```

In most cases, just ignore the caching layer — it's automatic and transparent to your code.

---

## Troubleshooting

### Gotcha 1: ExcelExporter Converts Values to ToString()

**Problem:** You export a list with numeric columns, but Excel shows them as text. Formulas referencing these columns won't work.

**Root Cause:** `ExcelExporter.PopulateSheet()` calls `value.ToString()` for all cell values (line 141), storing everything as text in the worksheet. Excel formulas cannot reference text representations of numbers.

**Example:**
```csharp
var data = new[] { new { Id = 1, Price = 99.99m } };
var result = await exporter.ExportAsync(data, "Items");
// Excel cell: "99.99" (text) — formulas won't work
```

**Solution:** Use numeric properties directly or pre-convert to string only for display:

```csharp
// BETTER: Keep numeric types
var data = orders.Select(o => new
{
    OrderId = o.Id,
    Total = o.Total,  // Decimal - Excel will treat as number
    Date = o.Date     // DateTime - Excel will format as date
});

var result = await exporter.ExportAsync(data, "Orders");
// Cells are now proper Excel numbers - formulas work!
```

**Alternative:** Create a DTO with calculated columns if you need specific formatting:

```csharp
public class OrderExportDto
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }  // Type is decimal, not string
    public DateTime Date { get; set; }   // Type is DateTime
}

var dtos = orders.Select(o => new OrderExportDto
{
    OrderId = o.Id,
    Total = o.Total,
    Date = o.Date
}).ToList();

var result = await exporter.ExportAsync(dtos, "Orders");
```

### Gotcha 2: Currency Format Triggers on Property Name Substring Match

**Problem:** Property names containing "price", "amount", or "currency" (case-insensitive) automatically trigger currency formatting. A property named "AmountDue" gets currency format even if it's not always numeric.

**Root Cause:** `ApplyDataFormatting()` in ExcelExporter checks `propertyName.ToLower().Contains("price")` (line 218), matching any substring. This applies currency format to properties like "TotalPrice", "OrderAmount", "Fee".

**Example:**
```csharp
var data = new[] { 
    new { OrderAmount = 100, Description = "Basic" },
    new { OrderAmount = 250, Description = "Premium" }
};

var result = await exporter.ExportAsync(data, "Orders");
// OrderAmount column formatted as currency ($100.00, $250.00)
```

**Solution:** Either use the naming convention intentionally, or rename properties to avoid substring matches:

```csharp
// OPTION 1: Use the convention intentionally
public class OrderExport
{
    public int OrderId { get; set; }
    public decimal TotalPrice { get; set; }  // Will auto-format as currency
    public string Status { get; set; }
}

// OPTION 2: Rename to avoid substring match
public class OrderExport
{
    public int OrderId { get; set; }
    public decimal TotalValue { get; set; }  // Won't trigger currency format
    public string Status { get; set; }
}

// Then manually format if needed:
var options = new ExcelOptions { CurrencyFormat = "$#,##0.00" };
var exporter = new ExcelExporter(options);
```

### Gotcha 3: PDF Page Sizing and Margin Configuration

**Problem:** Exported PDF has unexpected page size or margins don't apply as expected.

**Root Cause:** Page size string must match supported values (A4, Letter, A3, A5, Legal). Margins are in points (1 inch = 72 points). QuestPDF API may require additional configuration for custom page sizes.

**Solution:** Verify page size strings and use standard margin values:

```csharp
var options = new PdfOptions
{
    PageSize = "A4",           // Valid: A4, Letter, A3, A5, Legal
    Orientation = "Portrait",  // Portrait or Landscape
    TopMargin = 54,            // 0.75 inch (54 points)
    BottomMargin = 54,
    LeftMargin = 72,           // 1 inch (72 points)
    RightMargin = 72
};

var exporter = new PdfExporter(options);
var result = await exporter.ExportAsync(data, "Report");
```

Common point conversions:
- 0.5 inch = 36 points (default)
- 0.75 inch = 54 points
- 1 inch = 72 points
- 1.25 inch = 90 points

### Gotcha 4: Licensing and Library Dependencies

**Problem:** Do I need to pay for PDF or Excel export? Are there licensing restrictions?

**Root Cause:** Incorrect library choice or licensing concerns about iText or ClosedXML.

**Solution:** Both libraries are open-source and free:

| Library | License | Cost | Usage |
|---------|---------|------|-------|
| iText 7 | AGPL 3.0 / Commercial | Free (AGPL) | PDF generation, used by PdfExporter |
| ClosedXML | MIT | Free | Excel creation, used by ExcelExporter |

Both are free for open-source and commercial projects under their respective licenses. No licensing fees or per-export costs.

---

## Best Practices

1. **Always check Result<byte[]> for failures** — Export operations can fail (invalid data, memory pressure). Check `result.IsFailure` before returning files.
2. **Use DTOs for export** — Map domain models to export DTOs to control which properties appear in the output.
3. **Cache ExportOptions** — Create reusable option configurations and inject them as singleton services.
4. **Avoid large batch exports** — Export in pages for large datasets (use RowsPerPage in PdfOptions, implement pagination).
5. **Respect multi-tenancy** — Always filter exported data by tenant ID before exporting.
6. **Test currency and date formatting** — Currency and date formats depend on culture and property name conventions; test thoroughly.
7. **Use reflection caching** — The reflection caching is automatic; repeated exports of the same type benefit from cached PropertyInfo[].
