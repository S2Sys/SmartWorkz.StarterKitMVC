namespace SmartWorkz.Mobile.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Optimizes individual changes into efficient batches for sync.
/// Handles deduplication, pattern collapse, grouping, and batch splitting.
/// </summary>
public interface ISyncBatchOptimizer
{
    /// <summary>
    /// Create an optimized batch from a list of changes.
    /// Deduplicates, collapses patterns, groups by entity type, and orders by timestamp.
    /// </summary>
    /// <param name="changes">The list of changes to batch. Must not be null.</param>
    /// <returns>Result with the optimized SyncBatch.</returns>
    Task<Result<SyncBatch>> CreateBatchAsync(IReadOnlyList<SyncChange> changes);

    /// <summary>
    /// Create a batch with specified max change count constraint.
    /// If changes exceed maxChangeCount, takes only the first maxChangeCount items.
    /// </summary>
    /// <param name="changes">The list of changes to batch. Must not be null.</param>
    /// <param name="maxChangeCount">Maximum number of changes to include. Must be > 0.</param>
    /// <returns>Result with the optimized SyncBatch.</returns>
    Task<Result<SyncBatch>> CreateBatchAsync(
        IReadOnlyList<SyncChange> changes,
        int maxChangeCount);

    /// <summary>
    /// Split a list of changes into multiple batches if needed.
    /// First deduplicates, then splits into chunks of maxChangesPerBatch size.
    /// </summary>
    /// <param name="changes">The list of changes to split. Must not be null.</param>
    /// <param name="maxChangesPerBatch">Maximum changes per batch. Default 100.</param>
    /// <returns>Result with a read-only list of optimized SyncBatch objects.</returns>
    Task<Result<IReadOnlyList<SyncBatch>>> SplitIntoBatchesAsync(
        IReadOnlyList<SyncChange> changes,
        int maxChangesPerBatch = 100);

    /// <summary>
    /// Get estimated size of a batch in bytes.
    /// Uses JSON serialization to estimate actual payload size.
    /// </summary>
    /// <param name="batch">The batch to estimate. Must not be null.</param>
    /// <returns>Result with estimated size in bytes.</returns>
    Task<Result<long>> EstimateBatchSizeAsync(SyncBatch batch);
}
