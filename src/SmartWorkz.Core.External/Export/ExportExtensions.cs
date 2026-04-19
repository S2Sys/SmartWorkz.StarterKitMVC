namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Extension methods for fluent API export operations.
/// </summary>
public static class ExportExtensions
{
    /// <summary>
    /// Exports an enumerable collection to Excel format using fluent API.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="sheetName">The name of the Excel sheet.</param>
    /// <param name="options">Optional configuration options for Excel export.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    public static async Task<Result<byte[]>> ToExcelAsync<T>(
        this IEnumerable<T> data,
        string sheetName,
        ExcelOptions? options = null,
        CancellationToken ct = default)
    {
        var exporter = new ExcelExporter(options);
        return await exporter.ExportAsync(data, sheetName, ct);
    }

    /// <summary>
    /// Exports an enumerable collection to PDF format using fluent API.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="title">The title of the PDF document.</param>
    /// <param name="options">Optional configuration options for PDF export.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the PDF file bytes.</returns>
    public static async Task<Result<byte[]>> ToPdfAsync<T>(
        this IEnumerable<T> data,
        string title,
        PdfOptions? options = null,
        CancellationToken ct = default)
    {
        var exporter = new PdfExporter(options);
        return await exporter.ExportAsync(data, title, ct);
    }

    /// <summary>
    /// Exports multiple sheets to a single Excel workbook using fluent API.
    /// </summary>
    /// <param name="sheets">Dictionary where key is sheet name and value is enumerable data.</param>
    /// <param name="options">Optional configuration options for Excel export.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    public static async Task<Result<byte[]>> ToExcelAsync(
        this Dictionary<string, IEnumerable<object>> sheets,
        ExcelOptions? options = null,
        CancellationToken ct = default)
    {
        var exporter = new ExcelExporter(options);
        return await exporter.ExportMultipleAsync(sheets, ct);
    }
}
