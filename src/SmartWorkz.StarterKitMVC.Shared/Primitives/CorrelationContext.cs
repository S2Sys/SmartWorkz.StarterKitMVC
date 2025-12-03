namespace SmartWorkz.StarterKitMVC.Shared.Primitives;

/// <summary>
/// Provides access to the current request's correlation ID for distributed tracing.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets or sets the correlation ID for the current request.
    /// </summary>
    string? CorrelationId { get; set; }
}

/// <summary>
/// Default implementation of <see cref="ICorrelationContext"/>.
/// </summary>
/// <example>
/// <code>
/// // Inject ICorrelationContext via DI
/// public class AuditService
/// {
///     private readonly ICorrelationContext _correlation;
///     
///     public AuditService(ICorrelationContext correlation) => _correlation = correlation;
///     
///     public void LogAction(string action)
///     {
///         Console.WriteLine($"[{_correlation.CorrelationId}] {action}");
///     }
/// }
/// </code>
/// </example>
public sealed class CorrelationContext : ICorrelationContext
{
    /// <inheritdoc />
    public string? CorrelationId { get; set; }
}
