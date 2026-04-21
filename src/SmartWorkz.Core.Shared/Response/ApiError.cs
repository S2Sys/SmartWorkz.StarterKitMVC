namespace SmartWorkz.Shared;

/// <summary>
/// Structured error representation for API responses.
/// Provides code, message, and optional field-level error details.
/// </summary>
public sealed class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? FieldErrors { get; set; }

    public ApiError() { }

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public ApiError(string code, string message, Dictionary<string, string[]> fieldErrors)
        : this(code, message)
    {
        FieldErrors = fieldErrors;
    }

    /// <summary>Create from core Error type.</summary>
    public static ApiError FromError(Error error)
        => new(error.Code, error.Message);

    /// <summary>Create from validation errors.</summary>
    public static ApiError FromValidationErrors(Dictionary<string, string[]> errors)
        => new("VALIDATION.ERROR", "One or more validation errors occurred.", errors);

    /// <summary>Create from exception.</summary>
    public static ApiError FromException(Exception ex, string code = "INTERNAL_SERVER_ERROR")
        => new(code, ex.Message);
}
