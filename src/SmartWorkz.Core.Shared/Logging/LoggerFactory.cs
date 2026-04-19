namespace SmartWorkz.Core.Shared.Logging;

public sealed class LoggerFactory : ILoggerFactory
{
    private readonly Dictionary<string, ILogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        if (!_loggers.TryGetValue(categoryName, out var logger))
        {
            logger = new LoggerAdapter(categoryName);
            _loggers[categoryName] = logger;
        }
        return logger;
    }

    public ILogger CreateLogger<T>() where T : class
        => CreateLogger(typeof(T).FullName ?? typeof(T).Name);
}
