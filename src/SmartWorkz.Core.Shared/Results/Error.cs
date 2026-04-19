namespace SmartWorkz.Core.Shared.Results;

/// <summary>
/// Represents a structured error with a machine-readable code and human-readable message.
///
/// This is the canonical Error type. It replaces:
/// - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
/// - The ad-hoc string errors in Models.Result
///
/// Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
/// MessageKey maps to localization resource keys for UI display.
/// </summary>
public sealed class Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    // Common factory shorthands
    public static Error NotFound(string entity, object id)
        => new($"{entity.ToUpperInvariant()}.NOT_FOUND", $"{entity} with id '{id}' was not found.");

    public static Error Validation(string field, string message)
        => new($"VALIDATION.{field.ToUpperInvariant()}", message);

    public static Error Unauthorized(string? detail = null)
        => new("AUTH.UNAUTHORIZED", detail ?? "You are not authorized to perform this action.");

    public static Error Conflict(string message)
        => new("CONFLICT", message);

    public static Error FromException(Exception ex, string code)
        => new(code, ex.Message);

    public override string ToString() => $"[{Code}] {Message}";
}
