namespace SmartWorkz.Mobile;

public interface IErrorHandler
{
    Result HandleException(Exception ex);
    Result<T> HandleException<T>(Exception ex);
    Task<Result> HandleWithRetryAsync(Func<Task> operation, int maxRetries, CancellationToken ct = default);
    MobileError FormatError(Exception ex);
}
