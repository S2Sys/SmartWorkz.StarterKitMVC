using SmartWorkz.Extensions;
using Microsoft.Extensions.Logging;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Webhooks API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • GET /api/webhooks - List registered webhooks (paginated)
/// • GET /api/webhooks/{id} - Get webhook by ID
/// • POST /api/webhooks - Register new webhook
/// • PUT /api/webhooks/{id} - Update webhook
/// • DELETE /api/webhooks/{id} - Delete webhook
/// • POST /api/webhooks/{id}/test - Test webhook delivery
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // List registered webhooks
/// var webhooks = await client.Webhooks.ListAsync();
/// foreach (var hook in webhooks.Items ?? new())
///     Console.WriteLine($"- {hook.Url} (Events: {string.Join(", ", hook.Events)})");
///
/// // Register new webhook
/// var registerRequest = new CreateWebhookRequest(
///     Url: "https://myapp.com/webhooks/smartworkz",
///     Events: new List&lt;string&gt; { "user.created", "transaction.completed" },
///     IsActive: true,
///     RetryAttempts: 3,
///     TimeoutSeconds: 10);
/// var created = await client.Webhooks.CreateAsync(registerRequest);
/// Console.WriteLine($"Webhook registered: {created.Id}");
///
/// // Test webhook delivery
/// var testRequest = new TestWebhookRequest(
///     WebhookId: created.Id,
///     EventType: "user.created");
/// await client.Webhooks.TestAsync(testRequest);
/// Console.WriteLine("Test event sent");
///
/// // Update webhook
/// var updateRequest = new UpdateWebhookRequest(
///     Events: new List&lt;string&gt; { "user.created", "user.deleted" });
/// await client.Webhooks.UpdateAsync(created.Id, updateRequest);
/// </code>
/// </para>
/// </summary>
public interface IWebhooksClient
{
    /// <summary>
    /// Lists all registered webhooks with pagination.
    /// </summary>
    /// <param name="request">List query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated webhook list.</returns>
    Task<PaginatedResponse<GetWebhookResponse>> ListAsync(ListWebhooksRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook by ID.
    /// </summary>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Webhook information.</returns>
    Task<GetWebhookResponse> GetAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new webhook.
    /// </summary>
    /// <param name="request">Webhook registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registered webhook information.</returns>
    Task<GetWebhookResponse> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing webhook.
    /// </summary>
    /// <param name="webhookId">The webhook ID to update.</param>
    /// <param name="request">Webhook update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated webhook information.</returns>
    Task<GetWebhookResponse> UpdateAsync(string webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    /// <param name="webhookId">The webhook ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion response.</returns>
    Task<ApiResponse<string>> DeleteAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a test event to a webhook to verify connectivity.
    /// </summary>
    /// <param name="request">Test webhook request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test result response.</returns>
    Task<ApiResponse<string>> TestAsync(TestWebhookRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IWebhooksClient"/>.
/// </summary>
public class WebhooksClient : IWebhooksClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhooksClient>? _logger;

    public WebhooksClient(HttpClient httpClient, ILogger<WebhooksClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetWebhookResponse>> ListAsync(ListWebhooksRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ListWebhooksRequest();

        var queryParams = BuildQueryString(request);
        var url = $"/api/webhooks{queryParams}";

        _logger?.LogDebug("Listing webhooks from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<PaginatedResponse<GetWebhookResponse>>(cancellationToken);
    }

    public async Task<GetWebhookResponse> GetAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(webhookId))
            throw new ArgumentNullException(nameof(webhookId));

        var url = $"/api/webhooks/{Uri.EscapeDataString(webhookId)}";
        _logger?.LogDebug("Getting webhook {WebhookId}", webhookId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<GetWebhookResponse>(cancellationToken);
    }

    public async Task<GetWebhookResponse> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Creating webhook for URL: {Url} - Events: {Events}",
            request.Url, string.Join(", ", request.Events));

        var response = await _httpClient.PostAsJsonAsync("/api/webhooks", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadAsAsync<GetWebhookResponse>(cancellationToken);
        _logger?.LogInformation("Webhook created successfully: {WebhookId}", created.Id);

        return created;
    }

    public async Task<GetWebhookResponse> UpdateAsync(string webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(webhookId))
            throw new ArgumentNullException(nameof(webhookId));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var url = $"/api/webhooks/{Uri.EscapeDataString(webhookId)}";
        _logger?.LogInformation("Updating webhook {WebhookId}", webhookId);

        var response = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadAsAsync<GetWebhookResponse>(cancellationToken);
        _logger?.LogInformation("Webhook updated successfully: {WebhookId}", webhookId);

        return updated;
    }

    public async Task<ApiResponse<string>> DeleteAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(webhookId))
            throw new ArgumentNullException(nameof(webhookId));

        var url = $"/api/webhooks/{Uri.EscapeDataString(webhookId)}";
        _logger?.LogInformation("Deleting webhook {WebhookId}", webhookId);

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger?.LogInformation("Webhook deleted successfully: {WebhookId}", webhookId);

        return new ApiResponse<string>(Success: true, Data: webhookId, Message: "Webhook deleted successfully");
    }

    public async Task<ApiResponse<string>> TestAsync(TestWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var url = $"/api/webhooks/{Uri.EscapeDataString(request.WebhookId)}/test";
        _logger?.LogInformation("Testing webhook {WebhookId} with event: {EventType}",
            request.WebhookId, request.EventType);

        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsAsync<ApiResponse<string>>(cancellationToken);
        _logger?.LogInformation("Webhook test completed for {WebhookId}", request.WebhookId);

        return result;
    }

    private static string BuildQueryString(ListWebhooksRequest request)
    {
        var queries = new List<string>();

        if (request.PageNumber > 0)
            queries.Add($"pageNumber={request.PageNumber}");

        if (request.PageSize > 0)
            queries.Add($"pageSize={request.PageSize}");

        return queries.Count > 0 ? $"?{string.Join("&", queries)}" : string.Empty;
    }
}
