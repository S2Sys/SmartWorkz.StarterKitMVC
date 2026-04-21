namespace SmartWorkz.Shared;

/// <summary>
/// Immutable audit log entry for tracking entity changes and domain events.
/// Records who did what, when, where, and why for compliance and debugging.
/// </summary>
public class AuditEntry
{
    /// <summary>Unique identifier for this audit entry.</summary>
    public Guid Id { get; set; }

    /// <summary>Entity type being audited (e.g., "Order", "User", "Invoice").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Entity instance identifier (e.g., order ID, user ID).</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Action performed (e.g., "Created", "Updated", "Deleted", "EventPublished").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>User who performed the action (null for system actions).</summary>
    public string? UserId { get; set; }

    /// <summary>IP address from which the action was performed.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Changed data: before/after values, or event-specific details.</summary>
    public Dictionary<string, object>? Changes { get; set; }

    /// <summary>Correlation ID linking related operations (distributed tracing).</summary>
    public string? CorrelationId { get; set; }

    /// <summary>OpenTelemetry trace ID for request tracing.</summary>
    public string? TraceId { get; set; }

    /// <summary>When the action occurred.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Reason code for audit trail analysis (e.g., "DomainEvent", "UserAction", "SystemJob").</summary>
    public string? ReasonCode { get; set; }
}
