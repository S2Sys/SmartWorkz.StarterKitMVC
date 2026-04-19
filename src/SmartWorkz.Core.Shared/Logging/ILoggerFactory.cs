namespace SmartWorkz.Core.Shared.Logging;

/// <summary>
/// Factory for creating logger instances by category/source.
/// </summary>
public interface ILoggerFactory
{
    ILogger CreateLogger(string categoryName);
    ILogger CreateLogger<T>() where T : class;
}
