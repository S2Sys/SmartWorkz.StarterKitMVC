namespace SmartWorkz.Core.Results;

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
