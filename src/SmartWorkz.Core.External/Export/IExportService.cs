namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Defines contract for generic CSV export operations using CsvHelper.
/// Supports automatic property discovery via reflection and special character handling.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports an enumerable collection to CSV format as a byte array.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export. Must not be null.</param>
    /// <param name="options">Optional configuration for the export. If null, default options are used.</param>
    /// <param name="cancellationToken">Cancellation token to support async operation cancellation.</param>
    /// <returns>A Result containing the CSV file bytes if successful; otherwise a failure result.</returns>
    /// <remarks>
    /// Features:
    /// - Automatic header generation from property names
    /// - Special character escaping (quotes, commas, newlines)
    /// - Generic collection support via reflection
    /// - Configurable delimiter and culture settings
    /// - Returns empty collection results as failures
    /// </remarks>
    Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        ExportOptions? options = null,
        CancellationToken cancellationToken = default);
}
