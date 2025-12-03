using System.Net;
using SmartWorkz.StarterKitMVC.Shared.Primitives;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Http;

/// <summary>
/// Represents an HTTP API response with typed data.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
/// <example>
/// <code>
/// // Success response
/// var success = new ApiResponse&lt;User&gt;
/// {
///     IsSuccess = true,
///     Data = new User { Name = "John" },
///     StatusCode = HttpStatusCode.OK
/// };
/// 
/// // Error response
/// var error = new ApiResponse&lt;User&gt;
/// {
///     IsSuccess = false,
///     Error = new ApiError { Code = "NOT_FOUND", Message = "User not found" },
///     StatusCode = HttpStatusCode.NotFound
/// };
/// </code>
/// </example>
public sealed record ApiResponse<T>
{
    /// <summary>Whether the request was successful.</summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>Response data (null on error).</summary>
    public T? Data { get; init; }
    
    /// <summary>Error details (null on success).</summary>
    public ApiError? Error { get; init; }
    
    /// <summary>HTTP status code.</summary>
    public HttpStatusCode StatusCode { get; init; }
}
