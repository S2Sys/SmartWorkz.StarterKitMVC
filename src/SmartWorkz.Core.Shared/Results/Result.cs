namespace SmartWorkz.Core.Shared.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
///
/// This unifies:
/// - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
/// - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
///
/// Design choice — class over struct:
/// 1. Result&lt;T&gt; inherits from Result to reuse Succeeded/Errors without duplication.
///    Structs cannot use inheritance this way.
/// 2. Services return Result from interface methods — class semantics (null check) are
///    simpler than boxing/unboxing structs across interface boundaries.
/// 3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
///    A single Error struct cannot carry multiple field errors.
///
/// The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
/// where you want zero-allocation returns. This class is for service layer contracts.
/// </summary>
public class Result
{
    // Backing field prevents subclasses from accidentally writing Succeeded = true on a failure
    private readonly bool _succeeded;

    public bool Succeeded => _succeeded;

    /// <summary>
    /// Localization resource key for the primary message (e.g. "Error.UserNotFound").
    /// Null on success.
    /// </summary>
    public string? MessageKey { get; protected init; }

    /// <summary>
    /// Structured error with code + message. Null on success.
    /// Bridges the Primitives.Error pattern for callers that check result.Error.Code.
    /// </summary>
    public Error? Error { get; protected init; }

    /// <summary>
    /// Field-level validation errors. Always non-null (empty list on success/non-validation failures).
    /// Used by ModelState.AddErrors() in Razor Pages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; protected init; } = [];

    protected Result(bool succeeded) => _succeeded = succeeded;

    // --- Factory methods ---

    public static Result Ok()
        => new(true);

    /// <summary>Failure with a localization message key and optional field-level error strings.</summary>
    public static Result Fail(string messageKey, params string[] errors)
        => new(false) { MessageKey = messageKey, Errors = errors };

    /// <summary>Failure from a structured Error (bridges the Primitives.Error pattern).</summary>
    public static Result Fail(Error error)
        => new(false) { Error = error, MessageKey = error.Code, Errors = [error.Message] };

    /// <summary>Factory for a typed result. Use in services that return data.</summary>
    public static Result<T> Ok<T>(T data)
        => new(true) { Data = data };

    public static Result<T> Fail<T>(string messageKey, params string[] errors)
        => new(false) { MessageKey = messageKey, Errors = errors };

    public static Result<T> Fail<T>(Error error)
        => new(false) { Error = error, MessageKey = error.Code, Errors = [error.Message] };
}

/// <summary>
/// Result with a typed payload. Data is only valid when Succeeded = true.
///
/// Usage:
///   Result&lt;UserDto&gt; result = await _userService.GetByIdAsync(id);
///   if (!result.Succeeded) return RedirectToPage("Error");
///   var user = result.Data!;
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; init; }

    internal Result(bool succeeded) : base(succeeded) { }
}
