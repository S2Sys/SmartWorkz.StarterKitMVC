namespace SmartWorkz.Shared;

/// <summary>
/// Service for recording and querying immutable audit entries.
/// Abstracts the persistence mechanism for audit trails.
/// </summary>
public interface IAuditTrail
{
    /// <summary>
    /// Record an audit entry (immutable append-only).
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all audit entries for a specific entity instance.
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "Order").</param>
    /// <param name="entityId">Entity instance ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyCollection<AuditEntry>> GetEntriesAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit entries by action type (Created, Updated, Deleted, etc.).
    /// </summary>
    /// <param name="action">The action to filter by.</param>
    /// <param name="since">Optional: only entries after this date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyCollection<AuditEntry>> GetEntriesByActionAsync(string action, DateTimeOffset? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit entries for a specific user.
    /// </summary>
    /// <param name="userId">User ID who performed actions.</param>
    /// <param name="since">Optional: only entries after this date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyCollection<AuditEntry>> GetEntriesByUserAsync(string userId, DateTimeOffset? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search audit trail with multiple filter criteria.
    /// All criteria are AND'd together (null criteria are ignored).
    /// </summary>
    /// <param name="entityType">Optional entity type filter.</param>
    /// <param name="action">Optional action filter.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="since">Optional timestamp filter (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyCollection<AuditEntry>> SearchAsync(
        string? entityType = null,
        string? action = null,
        string? userId = null,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}
