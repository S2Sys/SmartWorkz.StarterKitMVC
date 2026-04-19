namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Defines contract for exporting data to Excel format.
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// Exports enumerable data to a single Excel sheet.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="sheetName">The name of the Excel sheet.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    Task<Result<byte[]>> ExportAsync<T>(IEnumerable<T> data, string sheetName, CancellationToken ct = default);

    /// <summary>
    /// Exports multiple collections to multiple sheets in a single workbook.
    /// </summary>
    /// <param name="sheets">Dictionary where key is sheet name and value is enumerable data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the Excel file bytes.</returns>
    Task<Result<byte[]>> ExportMultipleAsync(Dictionary<string, IEnumerable<object>> sheets, CancellationToken ct = default);
}
