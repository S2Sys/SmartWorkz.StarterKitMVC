namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents a single change to an entity during sync.
/// </summary>
public sealed record SyncChange(
    string EntityId,
    string EntityType,
    string Property,
    object? OldValue,
    object? NewValue,
    DateTime Timestamp,
    string UserId,
    string ChangeId = "") : IEquatable<SyncChange>
{
    /// <summary>
    /// Unique identifier for this change.
    /// </summary>
    public string ChangeId { get; } = string.IsNullOrEmpty(ChangeId) ? Guid.NewGuid().ToString() : ChangeId;

    /// <summary>
    /// Get display name (Entity: Property = NewValue).
    /// </summary>
    public string DisplayName => $"{EntityType}({EntityId}): {Property} = {NewValue}";

    /// <summary>
    /// Check if this is a delete operation.
    /// </summary>
    public bool IsDelete => NewValue == null && OldValue != null;

    /// <summary>
    /// Check if this is a create operation.
    /// </summary>
    public bool IsCreate => OldValue == null && NewValue != null;

    /// <summary>
    /// Check if this is an update operation.
    /// </summary>
    public bool IsUpdate => OldValue != null && NewValue != null;
}
