using Microsoft.Extensions.Logging;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Logging;

/// <summary>
/// Default implementation of <see cref="ILoggerAdapter{T}"/> using Microsoft.Extensions.Logging.
/// </summary>
/// <typeparam name="T">The type to use as the logger category.</typeparam>
public sealed class LoggerAdapter<T> : ILoggerAdapter<T>
{
    private readonly ILogger<T> _logger;

    public LoggerAdapter(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args) => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args) => _logger.LogWarning(message, args);

    public void LogError(Exception exception, string message, params object[] args) => _logger.LogError(exception, message, args);
}
