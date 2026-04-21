namespace SmartWorkz.Shared;

public sealed class LoggerAdapter : ILogger
{
    private readonly string _categoryName;
    private LogLevel _minLevel = LogLevel.Information;

    public LoggerAdapter(string categoryName) => _categoryName = categoryName;

    public void LogInformation(string message, params object?[] args) => Log(LogLevel.Information, message, args);
    public void LogWarning(string message, params object?[] args) => Log(LogLevel.Warning, message, args);
    public void LogError(string message, Exception? exception = null, params object?[] args) => Log(LogLevel.Error, message, args, exception);
    public void LogDebug(string message, params object?[] args) => Log(LogLevel.Debug, message, args);
    public void LogCritical(string message, Exception? exception = null, params object?[] args) => Log(LogLevel.Critical, message, args, exception);
    public bool IsEnabled(LogLevel level) => level >= _minLevel;

    private void Log(LogLevel level, string message, object?[]? args, Exception? exception = null)
    {
        if (!IsEnabled(level)) return;
        var formattedMessage = args?.Length > 0 ? string.Format(message, args) : message;
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] [{level}] [{_categoryName}] {formattedMessage}";
        if (exception != null) logEntry += $"\nException: {exception}";
        if (level >= LogLevel.Error)
            Console.Error.WriteLine(logEntry);
        else
            System.Diagnostics.Debug.WriteLine(logEntry);
    }
}
