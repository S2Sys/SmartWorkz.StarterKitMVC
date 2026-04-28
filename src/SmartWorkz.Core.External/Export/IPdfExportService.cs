namespace SmartWorkz.Core.External.Export;

/// <summary>
/// Defines contract for exporting data to PDF format using iText7.
/// Supports automatic property discovery, professional table formatting, and styling.
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Exports enumerable data to a PDF document with professional table layout.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="title">The title of the PDF document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the PDF file bytes.</returns>
    /// <remarks>
    /// Features:
    /// - Professional table layout with headers
    /// - Header row formatting (bold text, gray background)
    /// - Data rows with proper alignment (numeric/date right-aligned, text left-aligned)
    /// - Number format (currency for price/amount, percentage for percentages)
    /// - Date format (yyyy-MM-dd HH:mm)
    /// - Configurable margins and padding
    /// - Page sizing support (A4, Letter, A3, A5, Legal)
    /// - Page numbers support
    /// - Special character handling
    /// - Returns failure for empty collections
    /// </remarks>
    Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        string title,
        CancellationToken cancellationToken = default);
}
