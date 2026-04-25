using SmartWorkz.Extensions;
using Microsoft.Extensions.Logging;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Transactions API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • GET /api/transactions - List transactions (paginated)
/// • GET /api/transactions/{id} - Get transaction by ID
/// • POST /api/transactions - Create new transaction
/// • GET /api/transactions/{id}/status - Get transaction status
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // List transactions with date filtering
/// var listRequest = new ListTransactionsRequest(
///     PageNumber: 1,
///     PageSize: 25,
///     UserId: "user-123",
///     Status: "completed");
/// var transactions = await client.Transactions.ListAsync(listRequest);
///
/// // Get specific transaction
/// var transaction = await client.Transactions.GetAsync("txn-456");
/// Console.WriteLine($"Amount: {transaction.Amount} {transaction.Currency}");
/// Console.WriteLine($"Status: {transaction.Status}");
///
/// // Create transaction
/// var createRequest = new CreateTransactionRequest(
///     UserId: "user-123",
///     Amount: 99.99m,
///     Currency: "USD",
///     Type: "payment",
///     Description: "Monthly subscription");
/// var created = await client.Transactions.CreateAsync(createRequest);
/// </code>
/// </para>
/// </summary>
public interface ITransactionsClient
{
    /// <summary>
    /// Lists all transactions with pagination and optional filtering.
    /// </summary>
    /// <param name="request">List query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated transaction list.</returns>
    Task<PaginatedResponse<GetTransactionResponse>> ListAsync(ListTransactionsRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a transaction by ID.
    /// </summary>
    /// <param name="transactionId">The transaction ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Transaction information.</returns>
    Task<GetTransactionResponse> GetAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="request">Transaction creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created transaction information.</returns>
    Task<GetTransactionResponse> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a transaction.
    /// </summary>
    /// <param name="transactionId">The transaction ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Transaction status information.</returns>
    Task<ApiResponse<string>> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="ITransactionsClient"/>.
/// </summary>
public class TransactionsClient : ITransactionsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TransactionsClient>? _logger;

    public TransactionsClient(HttpClient httpClient, ILogger<TransactionsClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetTransactionResponse>> ListAsync(ListTransactionsRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ListTransactionsRequest();

        var queryParams = BuildQueryString(request);
        var url = $"/api/transactions{queryParams}";

        _logger?.LogDebug("Listing transactions from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<PaginatedResponse<GetTransactionResponse>>(cancellationToken);
    }

    public async Task<GetTransactionResponse> GetAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(transactionId))
            throw new ArgumentNullException(nameof(transactionId));

        var url = $"/api/transactions/{Uri.EscapeDataString(transactionId)}";
        _logger?.LogDebug("Getting transaction {TransactionId}", transactionId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<GetTransactionResponse>(cancellationToken);
    }

    public async Task<GetTransactionResponse> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Creating transaction for user {UserId} - Amount: {Amount} {Currency}",
            request.UserId, request.Amount, request.Currency);

        var response = await _httpClient.PostAsJsonAsync("/api/transactions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadAsAsync<GetTransactionResponse>(cancellationToken);
        _logger?.LogInformation("Transaction created successfully: {TransactionId}", created.Id);

        return created;
    }

    public async Task<ApiResponse<string>> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(transactionId))
            throw new ArgumentNullException(nameof(transactionId));

        var url = $"/api/transactions/{Uri.EscapeDataString(transactionId)}/status";
        _logger?.LogDebug("Getting transaction status {TransactionId}", transactionId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<ApiResponse<string>>(cancellationToken);
    }

    private static string BuildQueryString(ListTransactionsRequest request)
    {
        var queries = new List<string>();

        if (request.PageNumber > 0)
            queries.Add($"pageNumber={request.PageNumber}");

        if (request.PageSize > 0)
            queries.Add($"pageSize={request.PageSize}");

        if (!string.IsNullOrEmpty(request.UserId))
            queries.Add($"userId={Uri.EscapeDataString(request.UserId)}");

        if (!string.IsNullOrEmpty(request.Status))
            queries.Add($"status={Uri.EscapeDataString(request.Status)}");

        if (request.StartDate.HasValue)
            queries.Add($"startDate={request.StartDate:O}");

        if (request.EndDate.HasValue)
            queries.Add($"endDate={request.EndDate:O}");

        return queries.Count > 0 ? $"?{string.Join("&", queries)}" : string.Empty;
    }
}
