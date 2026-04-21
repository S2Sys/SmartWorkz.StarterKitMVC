namespace SmartWorkz.Mobile;

#if !WINDOWS
using SQLite;
#endif

public class ErrorHandler : IErrorHandler
{
    private readonly IMobileContext _mobileContext;
    private readonly ILogger _logger;

    public ErrorHandler(IMobileContext mobileContext, ILogger logger)
    {
        _mobileContext = Guard.NotNull(mobileContext, nameof(mobileContext));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Handles an exception and returns a Result.Fail with mapped error.
    /// </summary>
    public Result HandleException(Exception ex)
    {
        Guard.NotNull(ex, nameof(ex));

        var error = MapException(ex);
        return Result.Fail(error);
    }

    /// <summary>
    /// Handles an exception and returns a Result.Fail with mapped error.
    /// </summary>
    public Result<T> HandleException<T>(Exception ex)
    {
        Guard.NotNull(ex, nameof(ex));

        var error = MapException(ex);
        return Result.Fail<T>(error);
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry logic.
    /// </summary>
    public async Task<Result> HandleWithRetryAsync(Func<Task> operation, int maxRetries, CancellationToken ct = default)
    {
        Guard.NotNull(operation, nameof(operation));
        ct.ThrowIfCancellationRequested();

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation();
                return Result.Ok();
            }
            catch (Exception) when (attempt < maxRetries)
            {
                var delayMs = (int)(100 * Math.Pow(2, attempt));
                _logger.LogDebug($"Operation failed, retrying in {delayMs}ms (attempt {attempt + 1}/{maxRetries})");

                try
                {
                    await Task.Delay(delayMs, ct);
                }
                catch (OperationCanceledException)
                {
                    return Result.Fail(new Error("HTTP.TIMEOUT", "Operation cancelled during retry delay"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Operation failed after retries", ex);
                return HandleException(ex);
            }
        }

        return Result.Ok();
    }

    /// <summary>
    /// Formats an exception into a MobileError with context information.
    /// </summary>
    public MobileError FormatError(Exception ex)
    {
        Guard.NotNull(ex, nameof(ex));

        var error = MapException(ex);

        return new MobileError
        {
            Code = error.Code,
            Message = error.Message,
            StackTrace = ex.StackTrace,
            Platform = _mobileContext.Platform,
            DeviceId = _mobileContext.DeviceId,
            OccurredAt = DateTime.UtcNow,
            Context = new Dictionary<string, object>
            {
                { "ExceptionType", ex.GetType().Name },
                { "InnerException", ex.InnerException?.Message ?? "None" }
            }
        };
    }

    /// <summary>
    /// Maps an exception to an Error with appropriate code and message.
    /// </summary>
    private Error MapException(Exception ex)
    {
        return ex switch
        {
            HttpRequestException httpEx => new Error("HTTP.REQUEST_FAILED", httpEx.Message),
            TaskCanceledException => new Error("HTTP.TIMEOUT", "Request timeout"),
            JsonException jsonEx => new Error("SERIALIZATION.FAILED", jsonEx.Message),
            #if !WINDOWS
            SQLiteException sqliteEx => new Error("STORAGE.SQLITE_ERROR", sqliteEx.Message),
            #endif
            OperationCanceledException => new Error("HTTP.TIMEOUT", "Operation cancelled"),
            _ => new Error("MOBILE.UNEXPECTED", ex.Message)
        };
    }
}
