namespace SmartWorkz.StarterKitMVC.Shared.Primitives;

/// <summary>
/// RFC 7807 Problem Details format for HTTP error responses.
/// </summary>
public record ProblemDetailsResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? Instance = null,
    Dictionary<string, object>? Extensions = null,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Trace ID for correlating with logs.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static ProblemDetailsResponse ValidationError(
        string detail,
        IReadOnlyDictionary<string, string[]> errors,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/validation-error",
            Title: "One or more validation errors occurred.",
            Status: 400,
            Detail: detail,
            Instance: instance,
            Errors: errors)
        {
            TraceId = traceId
        };

    /// <summary>
    /// Creates an unauthorized error response.
    /// </summary>
    public static ProblemDetailsResponse Unauthorized(
        string detail,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/unauthorized",
            Title: "Unauthorized",
            Status: 401,
            Detail: detail,
            Instance: instance)
        {
            TraceId = traceId
        };

    /// <summary>
    /// Creates a forbidden error response.
    /// </summary>
    public static ProblemDetailsResponse Forbidden(
        string detail,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/forbidden",
            Title: "Forbidden",
            Status: 403,
            Detail: detail,
            Instance: instance)
        {
            TraceId = traceId
        };

    /// <summary>
    /// Creates a not found error response.
    /// </summary>
    public static ProblemDetailsResponse NotFound(
        string detail,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/not-found",
            Title: "Not Found",
            Status: 404,
            Detail: detail,
            Instance: instance)
        {
            TraceId = traceId
        };

    /// <summary>
    /// Creates a conflict error response.
    /// </summary>
    public static ProblemDetailsResponse Conflict(
        string detail,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/conflict",
            Title: "Conflict",
            Status: 409,
            Detail: detail,
            Instance: instance)
        {
            TraceId = traceId
        };

    /// <summary>
    /// Creates an internal server error response.
    /// </summary>
    public static ProblemDetailsResponse InternalServerError(
        string detail,
        string? instance = null,
        string? traceId = null) =>
        new(
            Type: "https://api.example.com/errors/internal-server-error",
            Title: "Internal Server Error",
            Status: 500,
            Detail: detail,
            Instance: instance)
        {
            TraceId = traceId
        };
}
