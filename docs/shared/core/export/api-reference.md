# Export API Reference

## Classes & Interfaces

### ExcelExporter

- **Namespace:** `SmartWorkz.Core.External.Export.ExcelExporter`
- **Summary:** Sealed implementation of IExcelExporter for exporting data to Excel format using ClosedXML.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ExcelExporter class.
  - Parameters:
    - `options`: Configuration options for Excel export. If null, default options are used.
    - `logger`: Optional logger for diagnostic information.
- **ExportAsync``1** - Exports enumerable data to a single Excel sheet.
- **ExportMultipleAsync** - Exports multiple collections to multiple sheets in a single workbook.
- **PopulateSheet** - Populates a worksheet with data from a collection.
- **ApplyHeaderStyle** - Applies header styling to a cell.
- **ApplyDataFormatting** - Applies data formatting based on the value type.
- **ApplyBorders** - Applies borders to a cell.

### ExcelOptions

- **Namespace:** `SmartWorkz.Core.External.Export.ExcelOptions`
- **Summary:** Configuration options for Excel export.

### ExportExtensions

- **Namespace:** `SmartWorkz.Core.External.Export.ExportExtensions`
- **Summary:** Extension methods for fluent API export operations.

#### Methods & Properties

- **ToExcelAsync``1** - Exports an enumerable collection to Excel format using fluent API.
  - Parameters:
    - `data`: The data to export.
    - `sheetName`: The name of the Excel sheet.
    - `options`: Optional configuration options for Excel export.
    - `ct`: Cancellation token.
  - Returns: A Result containing the Excel file bytes.
- **ToPdfAsync``1** - Exports an enumerable collection to PDF format using fluent API.
  - Parameters:
    - `data`: The data to export.
    - `title`: The title of the PDF document.
    - `options`: Optional configuration options for PDF export.
    - `ct`: Cancellation token.
  - Returns: A Result containing the PDF file bytes.
- **ToExcelAsync** - Exports multiple sheets to a single Excel workbook using fluent API.
  - Parameters:
    - `sheets`: Dictionary where key is sheet name and value is enumerable data.
    - `options`: Optional configuration options for Excel export.
    - `ct`: Cancellation token.
  - Returns: A Result containing the Excel file bytes.

### IExcelExporter

- **Namespace:** `SmartWorkz.Core.External.Export.IExcelExporter`
- **Summary:** Defines contract for exporting data to Excel format.

#### Methods & Properties

- **ExportAsync``1** - Exports enumerable data to a single Excel sheet.
  - Parameters:
    - `data`: The data to export.
    - `sheetName`: The name of the Excel sheet.
    - `ct`: Cancellation token.
  - Returns: A Result containing the Excel file bytes.
- **ExportMultipleAsync** - Exports multiple collections to multiple sheets in a single workbook.
  - Parameters:
    - `sheets`: Dictionary where key is sheet name and value is enumerable data.
    - `ct`: Cancellation token.
  - Returns: A Result containing the Excel file bytes.

### IPdfExporter

- **Namespace:** `SmartWorkz.Core.External.Export.IPdfExporter`
- **Summary:** Defines contract for exporting data to PDF format.

#### Methods & Properties

- **ExportAsync``1** - Exports enumerable data to a PDF document with table layout.
  - Parameters:
    - `data`: The data to export.
    - `title`: The title of the PDF document.
    - `ct`: Cancellation token.
  - Returns: A Result containing the PDF file bytes.

### PageNumberEventHandler

- **Namespace:** `SmartWorkz.Core.External.Export.PageNumberEventHandler`
- **Summary:** Event handler for adding page numbers to PDF pages.

### PdfExporter

- **Namespace:** `SmartWorkz.Core.External.Export.PdfExporter`
- **Summary:** Sealed implementation of IPdfExporter for exporting data to PDF format using iText 7.
            Provides free, open-source PDF export without platform limitations or font configuration requirements.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the PdfExporter class.
  - Parameters:
    - `options`: Configuration options for PDF export. If null, default options are used.
    - `logger`: Optional logger for error tracking.
- **GetCachedProperties** - Gets cached property metadata for a type to avoid repeated reflection calls.
- **ExportAsync``1** - Exports enumerable data to a PDF document with table layout using iText 7.
- **GeneratePdf``1** - Generates a PDF document from the provided data using iText 7.
- **GetPageDimensions** - Gets page dimensions based on configured page size and orientation.
- **FormatCellValue** - Formats the cell value for display in the PDF.
- **GetCellTextAlignment** - Gets the appropriate text alignment for a cell based on its value type.

### PdfOptions

- **Namespace:** `SmartWorkz.Core.External.Export.PdfOptions`
- **Summary:** Configuration options for PDF export.

