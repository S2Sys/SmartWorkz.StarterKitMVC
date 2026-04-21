namespace SmartWorkz.Shared;

/// <summary>
/// A sealed implementation of <see cref="ICorrelationContext"/> for distributed request tracing.
/// </summary>
public sealed class CorrelationContext : ICorrelationContext
{
    private readonly Dictionary<string, object> _properties = new();

    /// <summary>
    /// Initializes a new instance with a generated correlation ID.
    /// </summary>
    public CorrelationContext() : this(Guid.NewGuid().ToString("N")) { }

    /// <summary>
    /// Initializes a new instance with a specified correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID to use</param>
    public CorrelationContext(string correlationId)
    {
        CorrelationId = Guard.NotEmpty(correlationId, nameof(correlationId));
    }

    /// <summary>
    /// Initializes a child context from a parent context.
    /// </summary>
    private CorrelationContext(string correlationId, string parentCorrelationId) : this(correlationId)
    {
        ParentCorrelationId = parentCorrelationId;
    }

    public string CorrelationId { get; }
    public string? ParentCorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? TenantId { get; set; }
    public IReadOnlyDictionary<string, object> Properties => _properties.AsReadOnly();

    public void SetProperty(string key, object value)
    {
        Guard.NotEmpty(key, nameof(key));
        _properties[key] = value;
    }

    public bool TryGetProperty(string key, out object? value)
    {
        Guard.NotEmpty(key, nameof(key));
        return _properties.TryGetValue(key, out value);
    }

    public ICorrelationContext CreateChildContext()
    {
        var childContext = new CorrelationContext(Guid.NewGuid().ToString("N"), CorrelationId)
        {
            UserId = UserId,
            TenantId = TenantId
        };
        return childContext;
    }
}
