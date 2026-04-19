namespace SmartWorkz.Core.Shared.Response;

/// <summary>
/// Generic API response envelope that wraps result data with metadata.
/// Non-generic convenience version for non-data responses.
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ApiError? Error { get; set; }

    private ApiResponse() { }

    public ApiResponse(bool success, string? message = null, ApiError? error = null)
    {
        Success = success;
        Message = message;
        Error = error;
    }

    /// <summary>Success response without data.</summary>
    public static ApiResponse Ok(string? message = null)
        => new(true, message);

    /// <summary>Failure response with error details.</summary>
    public static ApiResponse Fail(ApiError error)
        => new(false, error.Message, error);

    /// <summary>Create from core Result pattern.</summary>
    public static ApiResponse FromResult(Result result)
        => result.Succeeded
            ? Ok(result.MessageKey)
            : Fail(ApiError.FromError(result.Error ?? new("ERROR", "An error occurred.")));
}

/// <summary>
/// Typed API response envelope with data payload.
/// Includes optional pagination metadata for list responses.
/// </summary>
/// <typeparam name="T">Type of payload data</typeparam>
public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public PaginationMetadata? Pagination { get; set; }

    private ApiResponse() { }

    public ApiResponse(bool success, T? data = default, string? message = null, ApiError? error = null)
    {
        Success = success;
        Data = data;
        Message = message;
        Error = error;
    }

    /// <summary>Success response with data.</summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new(true, data, message);

    /// <summary>Success response with paginated data.</summary>
    public static ApiResponse<T> OkPaginated(T data, PagedList<T> pagedList, string? message = null)
    {
        var response = new ApiResponse<T>(true, data, message)
        {
            Pagination = new PaginationMetadata(pagedList.Page, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages)
        };
        return response;
    }

    /// <summary>Failure response with error.</summary>
    public static ApiResponse<T> Fail(ApiError error)
        => new(false, error: error);

    /// <summary>Create from core Result<T> pattern.</summary>
    public static ApiResponse<T> FromResult(Result<T> result)
        => result.Succeeded
            ? Ok(result.Data!, result.MessageKey)
            : Fail(ApiError.FromError(result.Error ?? new("ERROR", "An error occurred.")));

    /// <summary>Nested pagination metadata for responses.</summary>
    public sealed class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public PaginationMetadata(int page, int pageSize, int totalCount, int totalPages)
        {
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = totalPages;
        }
    }
}
