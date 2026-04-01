namespace SmartWorkz.StarterKitMVC.Shared.Models;

/// <summary>
/// Uniform result type for all service methods.
/// Replaces thrown exceptions for expected business failures.
/// Use Result.Ok() / Result.Fail() / Result.Ok(data) in services.
/// Bind to page models via ModelState.AddErrors(result).
/// </summary>
public class Result
{
    public bool Succeeded { get; protected init; }
    public string? MessageKey { get; protected init; }
    public IReadOnlyList<string> Errors { get; protected init; } = [];

    public static Result Ok()
        => new() { Succeeded = true };

    public static Result Fail(string messageKey, params string[] errors)
        => new() { Succeeded = false, MessageKey = messageKey, Errors = errors };

    public static Result<T> Ok<T>(T data)
        => new() { Succeeded = true, Data = data };

    public static Result<T> Fail<T>(string messageKey, params string[] errors)
        => new() { Succeeded = false, MessageKey = messageKey, Errors = errors };
}

/// <summary>
/// Result with a typed payload. Data is only valid when Succeeded = true.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; init; }
}
