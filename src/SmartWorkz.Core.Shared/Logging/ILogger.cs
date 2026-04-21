namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for application logging.
/// Decouples from specific logging frameworks (Serilog, NLog, etc.).
/// </summary>
public interface ILogger
{
    void LogInformation(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogError(string message, Exception? exception = null, params object?[] args);
    void LogDebug(string message, params object?[] args);
    void LogCritical(string message, Exception? exception = null, params object?[] args);

    bool IsEnabled(LogLevel level);
}

/// <summary>Log level severity.</summary>
public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None
}
