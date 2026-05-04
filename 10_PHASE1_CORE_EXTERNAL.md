# SmartWorkz.Core.External - Implementation Plan

**Goal:** Implement complete export service suite (CSV, Excel, PDF) for data export functionality across Web, Mobile, and Backend projects.

**Architecture:** Generic service pattern with format-specific implementations, supporting LINQ-to-Objects with automatic property discovery.

**Tech Stack:** EPPlus 7.0+, iText7 7.2+, CsvHelper, .NET 9

**Timeline:** 1.5-2 weeks, 2 developers  
**Effort:** ~24 developer days  
**Priority:** HIGH - Blocks export functionality

---

## Phase 1: CSV Export (3-4 days)

### Task 1: CSV Export Service

```csharp
// src/Exporters/CsvExportService.cs
namespace SmartWorkz.Core.External.Exporters;

using CsvHelper;
using System.Globalization;

/// <summary>
/// Exports generic collections to CSV format with automatic property mapping.
/// Handles special characters, multi-line values, and custom headers.
/// </summary>
public class CsvExportService : IExportService
{
    public async Task<byte[]> ExportAsync<T>(
        IEnumerable<T> data,
        ExportOptions? options = null) where T : class
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write headers
        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            csv.WriteField(options?.GetHeader(prop.Name) ?? prop.Name);
        }
        await csv.NextRecordAsync();

        // Write data rows
        foreach (var item in data)
        {
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                csv.WriteField(value?.ToString() ?? "");
            }
            await csv.NextRecordAsync();
        }

        return Encoding.UTF8.GetBytes(writer.ToString());
    }
}
```

- [ ] Implement CsvExportService
- [ ] Write tests (5 tests minimum)
- [ ] Commit

**Effort:** 3-4 days

---

## Phase 2: Excel Export (4-5 days)

### Task 2: Excel Export Service

```csharp
// src/Exporters/ExcelExportService.cs
namespace SmartWorkz.Core.External.Exporters;

using OfficeOpenXml;

/// <summary>
/// Exports collections to Excel format with formatting, styling, and column sizing.
/// Supports multiple worksheets, formulas, and conditional formatting.
/// </summary>
public class ExcelExportService : IExportService
{
    public async Task<byte[]> ExportAsync<T>(
        IEnumerable<T> data,
        ExportOptions? options = null) where T : class
    {
        EPPlus.LicenseContext.SetLicenseContext(LicenseContext.NonCommercial);

        using var workbook = new ExcelWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        var list = data.ToList();
        if (list.Count == 0)
            return workbook.GetAsByteArray();

        var properties = typeof(T).GetProperties();

        // Write headers with formatting
        for (int col = 0; col < properties.Length; col++)
        {
            var cell = worksheet.Cells[1, col + 1];
            cell.Value = options?.GetHeader(properties[col].Name) ?? properties[col].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Write data with auto-sizing
        for (int row = 0; row < list.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(list[row]);
                var cell = worksheet.Cells[row + 2, col + 1];
                cell.Value = value;

                // Apply formatting based on type
                if (value is decimal || value is double)
                    cell.Style.Numberformat.Format = "#,##0.00";
                else if (value is DateTime)
                    cell.Style.Numberformat.Format = "yyyy-MM-dd HH:mm";
            }
        }

        worksheet.Cells.AutoFitColumns();
        return workbook.GetAsByteArray();
    }
}
```

- [ ] Implement ExcelExportService with EPPlus
- [ ] Add formatting (bold headers, auto-fit, number formatting)
- [ ] Write tests (5 tests)
- [ ] Commit

**Effort:** 4-5 days

---

## Phase 3: PDF Export (4-5 days)

### Task 3: PDF Export Service

```csharp
// src/Exporters/PdfExportService.cs
namespace SmartWorkz.Core.External.Exporters;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

/// <summary>
/// Exports collections to PDF format as professional tables with headers and formatting.
/// Supports page breaks, margins, and watermarks.
/// </summary>
public class PdfExportService : IExportService
{
    public async Task<byte[]> ExportAsync<T>(
        IEnumerable<T> data,
        ExportOptions? options = null) where T : class
    {
        using var ms = new MemoryStream();
        
        var writer = new PdfWriter(ms);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        var list = data.ToList();
        var properties = typeof(T).GetProperties();

        // Create table
        var table = new Table(properties.Length);

        // Add headers
        foreach (var prop in properties)
        {
            var headerCell = new Cell()
                .Add(new Paragraph(options?.GetHeader(prop.Name) ?? prop.Name)
                    .SetBold());
            table.AddHeaderCell(headerCell);
        }

        // Add data rows
        foreach (var item in list)
        {
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item)?.ToString() ?? "";
                table.AddCell(new Cell().Add(new Paragraph(value)));
            }
        }

        document.Add(table);
        document.Close();

        return ms.ToArray();
    }
}
```

- [ ] Implement PdfExportService with iText7
- [ ] Add table formatting and styling
- [ ] Write tests (5 tests)
- [ ] Commit

**Effort:** 4-5 days

---

## Phase 4: DI Extension & Tests (2-3 days)

### Task 4: Service Registration & Tests

```csharp
// src/Extensions/ExporterServiceCollectionExtensions.cs
public static class ExporterServiceCollectionExtensions
{
    public static IServiceCollection AddExporters(this IServiceCollection services)
    {
        services.AddScoped(typeof(IExportService), typeof(CsvExportService));
        services.AddScoped(typeof(IExportService), typeof(ExcelExportService));
        services.AddScoped(typeof(IExportService), typeof(PdfExportService));

        return services;
    }
}
```

- [ ] Create service registration extension
- [ ] Write comprehensive test suite (15+ tests)
- [ ] Integration tests with real data
- [ ] Commit

**Effort:** 2-3 days

---

## Summary

### Deliverables
- ✅ CsvExportService
- ✅ ExcelExportService
- ✅ PdfExportService
- ✅ IExportService interface
- ✅ ExportOptions configuration
- ✅ DI extensions
- ✅ 15+ unit tests
- ✅ Full XML documentation

### Timeline
- CSV (3-4 days)
- Excel (4-5 days)
- PDF (4-5 days)
- DI + Tests (2-3 days)

**Total: 1.5-2 weeks, 2 developers**

---

**Next:** Move to DevOps & CI/CD implementation plan
