namespace SmartWorkz.StarterKitMVC.Infrastructure.Logging;

/// <summary>
/// Logging abstraction for structured logging.
/// </summary>
/// <typeparam name="T">The type to use as the logger category.</typeparam>
/// <example>
/// <code>
/// public class OrderService
/// {
///     private readonly ILoggerAdapter&lt;OrderService&gt; _logger;
///     
///     public OrderService(ILoggerAdapter&lt;OrderService&gt; logger) => _logger = logger;
///     
///     public void ProcessOrder(Guid orderId)
///     {
///         _logger.LogInformation("Processing order {OrderId}", orderId);
///         try
///         {
///             // ... process ...
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
///             throw;
///         }
///     }
/// }
/// </code>
/// </example>
public interface ILoggerAdapter<T>
{
    /// <summary>Logs an informational message.</summary>
    void LogInformation(string message, params object[] args);
    
    /// <summary>Logs a warning message.</summary>
    void LogWarning(string message, params object[] args);
    
    /// <summary>Logs an error with exception details.</summary>
    void LogError(Exception exception, string message, params object[] args);
}
