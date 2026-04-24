namespace SmartWorkz.Mobile.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an optimized batch of changes ready for sync.
/// Contains deduplicated and pattern-collapsed changes grouped by entity type.
/// </summary>
public sealed record SyncBatch(
    string BatchId,
    IReadOnlyList<SyncChange> Changes,
    int ChangeCount,
    int DeduplicatedCount,
    DateTime CreatedAt,
    long ApproximateSizeBytes)
{
    /// <summary>
    /// Get batch description for logging.
    /// </summary>
    public string DisplayName => $"Batch {BatchId.Substring(0, Math.Min(8, BatchId.Length))}... ({ChangeCount} changes, {DeduplicatedCount} after dedup)";
}
