namespace SmartWorkz.Core;

/// <summary>
/// Represents a structured error with a machine-readable code and human-readable message.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string ToString() => $"[{Code}] {Message}";
}

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public class Result<T>
{
    private Result(bool succeeded, T? data, Error? error)
    {
        Succeeded = succeeded;
        Data = data;
        Error = error;
    }

    public bool Succeeded { get; }
    public T? Data { get; }
    public Error? Error { get; }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(Error error) => new(false, default, error);
}
