# SmartWorkz.Core.External — PDF & Excel Export

## Assembly Reference

**ProjectReference:** `src/SmartWorkz.Core.External/`  
**Namespace:** `SmartWorkz.Core.External`  
**Target Framework:** .NET 9  
**Key Dependencies:** ClosedXML v0.101.0 (Excel), iText7 v7.2.4 (PDF)

---

## Excel Export (XML Documented)

### IExcelExporter Signature

```csharp
Task<Result<byte[]>> ExportAsync<T>(
    IEnumerable<T> data,
    string sheetName,
    CancellationToken cancellationToken = default);
```

Returns Result<byte[]> containing XLSX bytes or error.

### ExcelOptions — Styling & Layout

IncludeHeaders (default true)  
HeaderBold (true), HeaderFontSize (12), DataFontSize (11)  
AlternateRowColor (true), AutoFitColumns (true)  
DateFormat ("yyyy-MM-dd"), CurrencyFormat ("$#,##0.00")  
FreezeHeaderRow (true), ApplyAutoFilter (true)

### Usage Example

```csharp
var result = await _excelExporter.ExportAsync(
    products,
    "Products",
    HttpContext.RequestAborted);

if (!result.IsSuccess)
{
    TempData["Error"] = result.ErrorMessage;
    return RedirectToPage();
}

return File(
    result.Data,
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    $"products-{DateTime.Now:yyyyMMdd}.xlsx");
```

### Performance

Handles 100K+ rows without memory issues (streaming write)  
Concurrent exports are safe (independent instances)  
For 1M+ rows, export in 100K page chunks

### DI Registration

```csharp
services.AddScoped<IExcelExporter, ExcelExporter>();
```

---

## PDF Export (XML Documented)

### IPdfExporter Signature

```csharp
Task<Result<byte[]>> ExportAsync<T>(
    IEnumerable<T> data,
    string documentTitle,
    CancellationToken cancellationToken = default);
```

### PdfOptions — Page Layout

PageSize (A3, A4, A5, Letter, Legal)  
Orientation ("Portrait" or "Landscape")  
Margins: Top (20mm), Bottom (20mm), Left (15mm), Right (15mm)  
IncludePageNumbers (true), RowsPerPage (30)  
DateFormat ("yyyy-MM-dd"), FontName ("Helvetica")  
FontSizeHeader (14), FontSizeData (11)

### Status

**PDF exporter is a stub** — interface exists but returns "Feature not implemented".  
Plan: Implement with QuestPDF (modern, flexible) or iText7 (mature).

---

## Result<T> — Unified Return Type (XML Documented)

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }      // "EXPORT_FAILED", etc.
    public string? ErrorMessage { get; set; }   // User-friendly
}
```

### Error Codes

EXPORT_FAILED — General export error  
INVALID_DATA — Data null or empty  
EXPORT_TIMEOUT — Exceeded timeout  
FILE_WRITE_ERROR — Disk write failed

---

**Next:** [07-smartworkz-mobile.md](./07-smartworkz-mobile.md)
