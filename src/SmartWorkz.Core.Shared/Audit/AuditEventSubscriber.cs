namespace SmartWorkz.Shared;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Subscribes to domain events and records them in the audit trail.
/// Enables automatic audit capture without requiring explicit audit calls in business logic.
/// </summary>
public class AuditEventSubscriber
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditEventSubscriber> _logger;

    public AuditEventSubscriber(IServiceProvider serviceProvider, ILogger<AuditEventSubscriber> logger)
    {
        _serviceProvider = Guard.NotNull(serviceProvider, nameof(serviceProvider));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Record a domain event in the audit trail.
    /// </summary>
    /// <param name="evt">The domain event to record.</param>
    /// <param name="userId">User ID who triggered the event (optional for system events).</param>
    /// <param name="ipAddress">IP address of the request originator (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task OnEventPublishedAsync(
        IDomainEvent evt,
        string? userId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(evt, nameof(evt));

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var auditTrail = scope.ServiceProvider.GetRequiredService<IAuditTrail>();

                var eventData = new Dictionary<string, object>
                {
                    { "EventType", evt.GetType().Name },
                    { "AggregateId", evt.AggregateId },
                    { "EventId", evt.EventId },
                    { "OccurredAt", evt.OccurredAt }
                };

                var entry = new AuditEntry
                {
                    Id = Guid.NewGuid(),
                    EntityType = evt.GetType().Name,
                    EntityId = evt.AggregateId,
                    Action = "EventPublished",
                    UserId = userId,
                    IpAddress = ipAddress,
                    Changes = eventData,
                    CorrelationId = evt.EventId.ToString(),
                    Timestamp = evt.OccurredAt,
                    ReasonCode = "DomainEvent"
                };

                await auditTrail.RecordAsync(entry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record audit for event {EventType}", evt.GetType().Name);
        }
    }
}
