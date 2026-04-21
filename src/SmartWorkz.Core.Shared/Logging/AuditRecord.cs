namespace SmartWorkz.Shared;

/// <summary>
/// Represents an immutable audit record with all relevant audit information.
/// </summary>
/// <param name="Id">The unique identifier of the audit record</param>
/// <param name="EntityType">The type of entity being audited (e.g., "User", "BlogPost")</param>
/// <param name="EntityId">The identifier of the audited entity</param>
/// <param name="Action">The action performed (Create, Update, Delete, etc.)</param>
/// <param name="UserId">The identifier of the user who performed the action</param>
/// <param name="PerformedAt">The timestamp when the action was performed</param>
/// <param name="Metadata">Optional metadata dictionary containing additional context</param>
public sealed record AuditRecord(
    string Id,
    string EntityType,
    string EntityId,
    string Action,
    string UserId,
    DateTime PerformedAt,
    Dictionary<string, object>? Metadata = null
);
