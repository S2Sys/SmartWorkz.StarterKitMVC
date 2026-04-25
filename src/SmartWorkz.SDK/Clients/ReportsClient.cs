using SmartWorkz.Extensions;
using Microsoft.Extensions.Logging;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Reports API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • GET /api/reports - List generated reports (paginated)
/// • GET /api/reports/{id} - Get report by ID
/// • POST /api/reports - Generate new report
/// • DELETE /api/reports/{id} - Delete report
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // List existing reports
/// var listRequest = new ListReportsRequest(PageNumber: 1, PageSize: 10, Type: "sales");
/// var reports = await client.Reports.ListAsync(listRequest);
/// Console.WriteLine($"Found {reports.TotalCount} reports");
///
/// // Generate new report
/// var generateRequest = new GenerateReportRequest(
///     Title: "Monthly Sales Report",
///     Type: "sales",
///     Format: "pdf",
///     Filters: new Dictionary&lt;string, object&gt;
///     {
///         { "month", 4 },
///         { "year", 2026 }
///     });
/// var generated = await client.Reports.GenerateAsync(generateRequest);
/// Console.WriteLine($"Report ID: {generated.Id}");
/// Console.WriteLine($"Download: {generated.DataUrl}");
///
/// // Get report details
/// var report = await client.Reports.GetAsync(generated.Id);
/// Console.WriteLine($"Records: {report.RecordCount}");
/// </code>
/// </para>
/// </summary>
public interface IReportsClient
{
    /// <summary>
    /// Lists all generated reports with pagination and optional filtering.
    /// </summary>
    /// <param name="request">List query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated report list.</returns>
    Task<PaginatedResponse<GetReportResponse>> ListAsync(ListReportsRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a report by ID.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Report information.</returns>
    Task<GetReportResponse> GetAsync(string reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a new report asynchronously.
    /// </summary>
    /// <param name="request">Report generation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated report information.</returns>
    Task<GetReportResponse> GenerateAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report.
    /// </summary>
    /// <param name="reportId">The report ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion response.</returns>
    Task<ApiResponse<string>> DeleteAsync(string reportId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IReportsClient"/>.
/// </summary>
public class ReportsClient : IReportsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReportsClient>? _logger;

    public ReportsClient(HttpClient httpClient, ILogger<ReportsClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetReportResponse>> ListAsync(ListReportsRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ListReportsRequest();

        var queryParams = BuildQueryString(request);
        var url = $"/api/reports{queryParams}";

        _logger?.LogDebug("Listing reports from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<PaginatedResponse<GetReportResponse>>(cancellationToken);
    }

    public async Task<GetReportResponse> GetAsync(string reportId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(reportId))
            throw new ArgumentNullException(nameof(reportId));

        var url = $"/api/reports/{Uri.EscapeDataString(reportId)}";
        _logger?.LogDebug("Getting report {ReportId}", reportId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<GetReportResponse>(cancellationToken);
    }

    public async Task<GetReportResponse> GenerateAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Generating report: {Title} (Format: {Format})", request.Title, request.Format);

        var response = await _httpClient.PostAsJsonAsync("/api/reports", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var generated = await response.Content.ReadAsAsync<GetReportResponse>(cancellationToken);
        _logger?.LogInformation("Report generated successfully: {ReportId} - Records: {RecordCount}", generated.Id, generated.RecordCount);

        return generated;
    }

    public async Task<ApiResponse<string>> DeleteAsync(string reportId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(reportId))
            throw new ArgumentNullException(nameof(reportId));

        var url = $"/api/reports/{Uri.EscapeDataString(reportId)}";
        _logger?.LogInformation("Deleting report {ReportId}", reportId);

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger?.LogInformation("Report deleted successfully: {ReportId}", reportId);

        return new ApiResponse<string>(Success: true, Data: reportId, Message: "Report deleted successfully");
    }

    private static string BuildQueryString(ListReportsRequest request)
    {
        var queries = new List<string>();

        if (request.PageNumber > 0)
            queries.Add($"pageNumber={request.PageNumber}");

        if (request.PageSize > 0)
            queries.Add($"pageSize={request.PageSize}");

        if (!string.IsNullOrEmpty(request.Type))
            queries.Add($"type={Uri.EscapeDataString(request.Type)}");

        return queries.Count > 0 ? $"?{string.Join("&", queries)}" : string.Empty;
    }
}
