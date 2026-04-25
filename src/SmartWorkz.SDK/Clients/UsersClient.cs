using Microsoft.Extensions.Logging;
using SmartWorkz.Extensions;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Users API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • GET /api/users - List all users (paginated)
/// • GET /api/users/{id} - Get user by ID
/// • POST /api/users - Create new user
/// • PUT /api/users/{id} - Update user
/// • DELETE /api/users/{id} - Delete user
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // List users with pagination
/// var request = new ListUsersRequest(PageNumber: 1, PageSize: 25);
/// var result = await client.Users.ListAsync(request);
/// Console.WriteLine($"Total users: {result.TotalCount}");
/// foreach (var user in result.Items ?? new())
///     Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");
///
/// // Get specific user
/// var user = await client.Users.GetAsync("user-123");
/// Console.WriteLine($"{user.FirstName} {user.LastName}");
///
/// // Create new user
/// var newUserRequest = new CreateUserRequest(
///     Email: "newuser@example.com",
///     FirstName: "John",
///     LastName: "Doe");
/// var created = await client.Users.CreateAsync(newUserRequest);
/// Console.WriteLine($"Created user: {created.Id}");
///
/// // Update user
/// var updateRequest = new UpdateUserRequest(FirstName: "Jane");
/// var updated = await client.Users.UpdateAsync("user-123", updateRequest);
///
/// // Delete user
/// await client.Users.DeleteAsync("user-123");
/// </code>
/// </para>
/// </summary>
public interface IUsersClient
{
    /// <summary>
    /// Lists all users with pagination and optional filtering.
    /// </summary>
    /// <param name="request">List query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated user list.</returns>
    Task<PaginatedResponse<GetUserResponse>> ListAsync(ListUsersRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User information.</returns>
    Task<GetUserResponse> GetAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">User creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user information.</returns>
    Task<GetUserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">The user ID to update.</param>
    /// <param name="request">User update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user information.</returns>
    Task<GetUserResponse> UpdateAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="userId">The user ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion result.</returns>
    Task<DeleteUserResponse> DeleteAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IUsersClient"/>.
/// </summary>
public class UsersClient : IUsersClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersClient>? _logger;

    public UsersClient(HttpClient httpClient, ILogger<UsersClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetUserResponse>> ListAsync(ListUsersRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ListUsersRequest();

        var queryParams = BuildQueryString(request);
        var url = $"/api/users{queryParams}";

        _logger?.LogDebug("Listing users from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<PaginatedResponse<GetUserResponse>>(cancellationToken);
    }

    public async Task<GetUserResponse> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        var url = $"/api/users/{Uri.EscapeDataString(userId)}";
        _logger?.LogDebug("Getting user {UserId}", userId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<GetUserResponse>(cancellationToken);
    }

    public async Task<GetUserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Creating user with email {Email}", request.Email);

        var response = await _httpClient.PostAsJsonAsync("/api/users", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadAsAsync<GetUserResponse>(cancellationToken);
        _logger?.LogInformation("User created successfully: {UserId}", created.Id);

        return created;
    }

    public async Task<GetUserResponse> UpdateAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var url = $"/api/users/{Uri.EscapeDataString(userId)}";
        _logger?.LogInformation("Updating user {UserId}", userId);

        var response = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadAsAsync<GetUserResponse>(cancellationToken);
        _logger?.LogInformation("User updated successfully: {UserId}", userId);

        return updated;
    }

    public async Task<DeleteUserResponse> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        var url = $"/api/users/{Uri.EscapeDataString(userId)}";
        _logger?.LogInformation("Deleting user {UserId}", userId);

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger?.LogInformation("User deleted successfully: {UserId}", userId);

        return new DeleteUserResponse(Success: true, Message: "User deleted successfully");
    }

    private static string BuildQueryString(ListUsersRequest request)
    {
        var queries = new List<string>();

        if (request.PageNumber > 0)
            queries.Add($"pageNumber={request.PageNumber}");

        if (request.PageSize > 0)
            queries.Add($"pageSize={request.PageSize}");

        if (!string.IsNullOrEmpty(request.SearchTerm))
            queries.Add($"search={Uri.EscapeDataString(request.SearchTerm)}");

        if (!string.IsNullOrEmpty(request.SortBy))
            queries.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

        if (request.SortDescending)
            queries.Add("sortDescending=true");

        return queries.Count > 0 ? $"?{string.Join("&", queries)}" : string.Empty;
    }
}
