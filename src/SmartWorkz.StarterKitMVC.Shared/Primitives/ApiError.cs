namespace SmartWorkz.StarterKitMVC.Shared.Primitives;

/// <summary>
/// Represents a standardized API error response.
/// </summary>
/// <example>
/// <code>
/// var error = new ApiError
/// {
///     Code = "VALIDATION_ERROR",
///     Message = "One or more validation errors occurred.",
///     TraceId = "abc-123",
///     Details = new Dictionary&lt;string, string[]&gt;
///     {
///         ["Email"] = new[] { "Email is required.", "Email format is invalid." }
///     }
/// };
/// </code>
/// </example>
public sealed class ApiError
{
    /// <summary>
    /// Unique error code for identification.
    /// </summary>
    public string Code { get; init; } = string.Empty;
    
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Correlation/trace ID for debugging.
    /// </summary>
    public string? TraceId { get; init; }
    
    /// <summary>
    /// Detailed validation errors by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}
