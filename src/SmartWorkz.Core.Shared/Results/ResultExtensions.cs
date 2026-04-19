namespace SmartWorkz.Core.Shared.Results;

/// <summary>
/// Functional helpers for chaining Result operations.
/// Keeps service code flat — avoids nested if (!result.Succeeded) blocks.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Transform the Data value if the result succeeded.</summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
        => result.Succeeded
            ? Result.Ok(map(result.Data!))
            : Result.Fail<TOut>(result.MessageKey ?? string.Empty, [.. result.Errors]);

    /// <summary>Chain a second operation that also returns Result.</summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next)
        => result.Succeeded
            ? await next(result.Data!)
            : Result.Fail<TOut>(result.MessageKey ?? string.Empty, [.. result.Errors]);

    /// <summary>Execute a side-effect action on success, then return the original result.</summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.Succeeded) action(result.Data!);
        return result;
    }

    /// <summary>Execute a side-effect action on failure, then return the original result.</summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Result<T>> action)
    {
        if (!result.Succeeded) action(result);
        return result;
    }
}
