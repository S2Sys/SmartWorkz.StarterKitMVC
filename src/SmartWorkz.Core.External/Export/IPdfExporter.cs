namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Defines contract for exporting data to PDF format.
/// </summary>
public interface IPdfExporter
{
    /// <summary>
    /// Exports enumerable data to a PDF document with table layout.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="title">The title of the PDF document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the PDF file bytes.</returns>
    Task<Result<byte[]>> ExportAsync<T>(IEnumerable<T> data, string title, CancellationToken ct = default);
}
