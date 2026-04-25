namespace SmartWorkz.Models;

/// <summary>
/// Generic API response wrapper for single-item responses.
/// </summary>
/// <typeparam name="T">The type of data in the response.</typeparam>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    string[]? Errors = null);

/// <summary>
/// Generic API response wrapper for paginated list responses.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public record PaginatedResponse<T>(
    bool Success,
    List<T>? Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    string? Message,
    string[]? Errors = null);

/// <summary>
/// Authentication response containing JWT token.
/// </summary>
public record AuthenticationResponse(
    bool Success,
    string? Token,
    string? RefreshToken,
    int? ExpiresIn,
    string? Message,
    string[]? Errors = null);

/// <summary>
/// User information response.
/// </summary>
public record GetUserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// List users query options.
/// </summary>
public record ListUsersRequest(
    int PageNumber = 1,
    int PageSize = 50,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false);

/// <summary>
/// Create user request.
/// </summary>
public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? Password = null);

/// <summary>
/// Update user request.
/// </summary>
public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null);

/// <summary>
/// Delete user response.
/// </summary>
public record DeleteUserResponse(
    bool Success,
    string? Message);

/// <summary>
/// Transaction information response.
/// </summary>
public record GetTransactionResponse(
    string Id,
    string UserId,
    decimal Amount,
    string Currency,
    string Status,
    string Type,
    string? Description,
    DateTime CreatedAt,
    DateTime? CompletedAt);

/// <summary>
/// List transactions query options.
/// </summary>
public record ListTransactionsRequest(
    int PageNumber = 1,
    int PageSize = 50,
    string? UserId = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

/// <summary>
/// Create transaction request.
/// </summary>
public record CreateTransactionRequest(
    string UserId,
    decimal Amount,
    string Currency,
    string Type,
    string? Description = null);

/// <summary>
/// Product information response.
/// </summary>
public record GetProductResponse(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// List products query options.
/// </summary>
public record ListProductsRequest(
    int PageNumber = 1,
    int PageSize = 50,
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);

/// <summary>
/// Create product request.
/// </summary>
public record CreateProductRequest(
    string Name,
    string? Description = null,
    decimal Price = 0m,
    int StockQuantity = 0);

/// <summary>
/// Update product request.
/// </summary>
public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    bool? IsActive = null);

/// <summary>
/// Report information response.
/// </summary>
public record GetReportResponse(
    string Id,
    string Title,
    string Type,
    string Format,
    int RecordCount,
    DateTime CreatedAt,
    string? DataUrl);

/// <summary>
/// List reports query options.
/// </summary>
public record ListReportsRequest(
    int PageNumber = 1,
    int PageSize = 50,
    string? Type = null);

/// <summary>
/// Generate report request.
/// </summary>
public record GenerateReportRequest(
    string Title,
    string Type,
    string Format = "pdf",
    Dictionary<string, object>? Filters = null);

/// <summary>
/// Webhook information response.
/// </summary>
public record GetWebhookResponse(
    string Id,
    string Url,
    List<string> Events,
    bool IsActive,
    DateTime CreatedAt,
    int? RetryAttempts,
    int? TimeoutSeconds);

/// <summary>
/// List webhooks query options.
/// </summary>
public record ListWebhooksRequest(
    int PageNumber = 1,
    int PageSize = 50);

/// <summary>
/// Create webhook request.
/// </summary>
public record CreateWebhookRequest(
    string Url,
    List<string> Events,
    bool IsActive = true,
    int? RetryAttempts = null,
    int? TimeoutSeconds = null);

/// <summary>
/// Update webhook request.
/// </summary>
public record UpdateWebhookRequest(
    string? Url = null,
    List<string>? Events = null,
    bool? IsActive = null,
    int? RetryAttempts = null,
    int? TimeoutSeconds = null);

/// <summary>
/// Webhook test request.
/// </summary>
public record TestWebhookRequest(
    string WebhookId,
    string EventType);

/// <summary>
/// Login request.
/// </summary>
public record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Refresh token request.
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken);

/// <summary>
/// Register request.
/// </summary>
public record RegisterRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password);

/// <summary>
/// Change password request.
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
