namespace SmartWorkz.StarterKitMVC.Shared.Primitives;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
/// <param name="Code">Unique error code for identification.</param>
/// <param name="Message">Human-readable error message.</param>
/// <example>
/// <code>
/// var error = new Error("USER_NOT_FOUND", "The specified user was not found.");
/// </code>
/// </example>
public readonly record struct Error(string Code, string Message)
{
    public static readonly Error None = new("None", string.Empty);
}

/// <summary>
/// Represents the result of an operation that does not return a value.
/// </summary>
/// <example>
/// <code>
/// // Success case
/// var success = Result.Success();
/// 
/// // Failure case
/// var failure = Result.Failure(new Error("VALIDATION_ERROR", "Invalid input."));
/// 
/// if (success.IsSuccess)
///     Console.WriteLine("Operation succeeded!");
/// </code>
/// </example>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <example>
/// <code>
/// // Success case
/// var result = Result&lt;User&gt;.Success(new User { Name = "John" });
/// 
/// // Failure case
/// var failure = Result&lt;User&gt;.Failure(new Error("NOT_FOUND", "User not found."));
/// 
/// if (result.IsSuccess)
///     Console.WriteLine($"User: {result.Value.Name}");
/// </code>
/// </example>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default, error);
}
