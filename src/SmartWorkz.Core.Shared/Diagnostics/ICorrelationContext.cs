namespace SmartWorkz.Shared;

/// <summary>
/// Defines a correlation context for distributed request tracing across systems.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets the correlation ID that uniquely identifies a request across distributed systems.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the parent correlation ID for distributed tracing hierarchies.
    /// </summary>
    string? ParentCorrelationId { get; }

    /// <summary>
    /// Gets the user ID associated with the correlation context.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the tenant ID associated with the correlation context for multi-tenant isolation.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets a collection of custom properties stored in the correlation context.
    /// </summary>
    IReadOnlyDictionary<string, object> Properties { get; }

    /// <summary>
    /// Adds or updates a property in the correlation context.
    /// </summary>
    void SetProperty(string key, object value);

    /// <summary>
    /// Attempts to retrieve a property from the correlation context.
    /// </summary>
    bool TryGetProperty(string key, out object? value);

    /// <summary>
    /// Creates a child correlation context for nested operations (for async/distributed flows).
    /// </summary>
    ICorrelationContext CreateChildContext();
}
