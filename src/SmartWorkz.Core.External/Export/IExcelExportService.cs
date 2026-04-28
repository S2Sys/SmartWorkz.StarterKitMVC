namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Defines contract for exporting data to Excel format using EPPlus.
/// Supports automatic property discovery, column sizing, formatting, and multiple worksheets.
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Exports enumerable data to a single Excel sheet with automatic formatting.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="sheetName">The name of the Excel sheet.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    /// <remarks>
    /// Features:
    /// - Automatic column width sizing based on content
    /// - Header formatting (bold text, gray background)
    /// - Number format (currency for price/amount, percentage for percentages)
    /// - Date format (yyyy-MM-dd HH:mm)
    /// - Special character handling
    /// - Returns failure for empty collections
    /// </remarks>
    Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports multiple collections to multiple sheets in a single workbook.
    /// </summary>
    /// <param name="sheets">Dictionary where key is sheet name and value is enumerable data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    /// <remarks>
    /// Features:
    /// - Multiple worksheets in single workbook
    /// - Consistent formatting across all sheets
    /// - Automatic sheet naming
    /// - Returns failure for empty sheets dictionary
    /// </remarks>
    Task<Result<byte[]>> ExportMultipleAsync(
        Dictionary<string, IEnumerable<object>> sheets,
        CancellationToken cancellationToken = default);
}
