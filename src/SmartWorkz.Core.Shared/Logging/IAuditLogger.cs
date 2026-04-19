namespace SmartWorkz.Core.Shared.Logging;

/// <summary>
/// Interface for structured audit logging with metadata support.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audit event with structured metadata.
    /// </summary>
    /// <param name="entityType">The entity type being audited (e.g., "User", "BlogPost")</param>
    /// <param name="entityId">The unique identifier of the entity</param>
    /// <param name="action">The action performed (Create, Update, Delete, etc.)</param>
    /// <param name="metadata">Optional metadata dictionary for additional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> LogAuditAsync(string entityType, string entityId, string action, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <param name="entityId">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of audit records</returns>
    Task<Result<IReadOnlyList<AuditRecord>>> GetAuditHistoryAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit logs for a specific user across all entities.
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of audit records</returns>
    Task<Result<IReadOnlyList<AuditRecord>>> GetUserActivityAsync(string userId, CancellationToken cancellationToken = default);
}
