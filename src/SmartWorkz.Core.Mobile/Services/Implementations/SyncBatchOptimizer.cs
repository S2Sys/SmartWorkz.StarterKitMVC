namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Optimizes individual changes into efficient batches for sync.
/// Handles deduplication (latest-write-wins), pattern collapse, grouping by entity type,
/// and ordering by timestamp for replay consistency.
/// </summary>
public class SyncBatchOptimizer : ISyncBatchOptimizer
{
    private readonly ILogger? _logger;
    private const int SizeEstimateOverheadPercent = 10;

    public SyncBatchOptimizer(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Create an optimized batch from a list of changes.
    /// </summary>
    public Task<Result<SyncBatch>> CreateBatchAsync(IReadOnlyList<SyncChange> changes)
    {
        Guard.NotNull(changes, nameof(changes));

        try
        {
            if (changes.Count == 0)
            {
                _logger?.LogDebug("Creating batch from empty change list");
                var emptyBatch = new SyncBatch(
                    BatchId: Guid.NewGuid().ToString(),
                    Changes: [],
                    ChangeCount: 0,
                    DeduplicatedCount: 0,
                    CreatedAt: DateTime.UtcNow,
                    ApproximateSizeBytes: 0);

                return Task.FromResult(Result.Ok(emptyBatch));
            }

            // Step 1: Collapse patterns first (must happen before dedup to detect transitions)
            var collapsed = CollapsePatterns(changes);

            // Step 2: Deduplicate changes - keep latest timestamp per (EntityId, Property)
            var deduplicated = DeduplicateChanges(collapsed);

            // Step 3: Group by EntityType, then order by timestamp DESC within each group
            var grouped = deduplicated
                .GroupBy(c => c.EntityType)
                .SelectMany(g => g.OrderByDescending(c => c.Timestamp))
                .ToList();

            // Step 4: Estimate size
            var sizeBytes = EstimateBatchSize(grouped);

            var batch = new SyncBatch(
                BatchId: Guid.NewGuid().ToString(),
                Changes: grouped,
                ChangeCount: changes.Count,
                DeduplicatedCount: grouped.Count,
                CreatedAt: DateTime.UtcNow,
                ApproximateSizeBytes: sizeBytes);

            _logger?.LogDebug(
                "Created batch {BatchId} with {ChangeCount} original changes, {DeduplicatedCount} after optimization",
                batch.BatchId.Substring(0, 8),
                batch.ChangeCount,
                batch.DeduplicatedCount);

            return Task.FromResult(Result.Ok(batch));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to create batch from {ChangeCount} changes", changes.Count);
            return Task.FromResult(
                Result.Fail<SyncBatch>("SyncBatch.CreateFailed", "Failed to create batch"));
        }
    }

    /// <summary>
    /// Create a batch with specified max change count constraint.
    /// </summary>
    public Task<Result<SyncBatch>> CreateBatchAsync(
        IReadOnlyList<SyncChange> changes,
        int maxChangeCount)
    {
        Guard.NotNull(changes, nameof(changes));

        if (maxChangeCount <= 0)
        {
            _logger?.LogWarning("Invalid maxChangeCount: {MaxChangeCount}", maxChangeCount);
            return Task.FromResult(
                Result.Fail<SyncBatch>(
                    "SyncBatch.InvalidMaxCount",
                    "maxChangeCount must be greater than 0"));
        }

        try
        {
            if (changes.Count <= maxChangeCount)
            {
                _logger?.LogDebug(
                    "Change count {Count} within limit {MaxCount}, creating standard batch",
                    changes.Count,
                    maxChangeCount);
                return CreateBatchAsync(changes);
            }

            // Take only the first maxChangeCount changes
            var limited = changes.Take(maxChangeCount).ToList();
            _logger?.LogDebug(
                "Limiting changes from {Total} to {Limited}",
                changes.Count,
                limited.Count);

            return CreateBatchAsync(limited);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to create batch with max count {MaxCount}",
                maxChangeCount);
            return Task.FromResult(
                Result.Fail<SyncBatch>("SyncBatch.CreateFailed", "Failed to create batch"));
        }
    }

    /// <summary>
    /// Split a list of changes into multiple batches if needed.
    /// </summary>
    public async Task<Result<IReadOnlyList<SyncBatch>>> SplitIntoBatchesAsync(
        IReadOnlyList<SyncChange> changes,
        int maxChangesPerBatch = 100)
    {
        Guard.NotNull(changes, nameof(changes));

        if (maxChangesPerBatch <= 0)
        {
            _logger?.LogWarning("Invalid maxChangesPerBatch {MaxChangesPerBatch}", maxChangesPerBatch);
            return Result.Fail<IReadOnlyList<SyncBatch>>(
                "SyncBatch.InvalidMaxCount",
                "maxChangesPerBatch must be greater than 0");
        }

        try
        {
            // First collapse patterns, then deduplicate
            var collapsed = CollapsePatterns(changes);
            var deduplicated = DeduplicateChanges(collapsed);

            if (deduplicated.Count == 0)
            {
                _logger?.LogDebug("No changes to split after optimization");
                var emptyBatch = new SyncBatch(
                    BatchId: Guid.NewGuid().ToString(),
                    Changes: [],
                    ChangeCount: changes.Count,
                    DeduplicatedCount: 0,
                    CreatedAt: DateTime.UtcNow,
                    ApproximateSizeBytes: 0);

                return Result.Ok<IReadOnlyList<SyncBatch>>(new[] { emptyBatch });
            }

            // Split into chunks
            var batches = new List<SyncBatch>();
            var chunks = deduplicated
                .Chunk(maxChangesPerBatch)
                .ToList();

            _logger?.LogDebug(
                "Splitting {Total} optimized changes into {BatchCount} batches of max {MaxPer}",
                deduplicated.Count,
                chunks.Count,
                maxChangesPerBatch);

            foreach (var chunk in chunks)
            {
                var grouped = chunk
                    .GroupBy(c => c.EntityType)
                    .SelectMany(g => g.OrderByDescending(c => c.Timestamp))
                    .ToList();

                var sizeBytes = EstimateBatchSize(grouped);

                var batch = new SyncBatch(
                    BatchId: Guid.NewGuid().ToString(),
                    Changes: grouped,
                    ChangeCount: chunk.Length,
                    DeduplicatedCount: grouped.Count,
                    CreatedAt: DateTime.UtcNow,
                    ApproximateSizeBytes: sizeBytes);

                batches.Add(batch);
            }

            return Result.Ok<IReadOnlyList<SyncBatch>>(batches);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to split {ChangeCount} changes into batches",
                changes.Count);
            return Result.Fail<IReadOnlyList<SyncBatch>>(
                "SyncBatch.SplitFailed",
                "Failed to split changes into batches");
        }
    }

    /// <summary>
    /// Get estimated size of a batch in bytes.
    /// </summary>
    public Task<Result<long>> EstimateBatchSizeAsync(SyncBatch batch)
    {
        Guard.NotNull(batch, nameof(batch));

        try
        {
            var json = JsonSerializer.Serialize(batch.Changes);
            var sizeBytes = (long)(System.Text.Encoding.UTF8.GetByteCount(json) * (1.0 + SizeEstimateOverheadPercent / 100.0));

            _logger?.LogDebug(
                "Estimated batch size: {SizeBytes} bytes ({ChangeCount} changes)",
                sizeBytes,
                batch.Changes.Count);

            return Task.FromResult(Result.Ok(sizeBytes));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to estimate batch size");
            return Task.FromResult(
                Result.Fail<long>("SyncBatch.EstimateFailed", "Failed to estimate batch size"));
        }
    }

    /// <summary>
    /// Deduplicate changes by (EntityId, Property), keeping latest timestamp.
    /// Note: Pattern collapse should be done first to detect Create+Update, Update+Delete, etc.
    /// This method is called after pattern collapse.
    /// </summary>
    private List<SyncChange> DeduplicateChanges(IReadOnlyList<SyncChange> changes)
    {
        _logger?.LogDebug("Deduplicating {Count} changes", changes.Count);

        // After pattern collapse, there should be at most one change per (EntityId, Property)
        // But we do this again to be safe in case of edge cases
        var deduplicated = changes
            .GroupBy(c => new { c.EntityId, c.Property })
            .Select(g => g.OrderByDescending(c => c.Timestamp).First())
            .ToList();

        return deduplicated;
    }

    /// <summary>
    /// Collapse patterns by (EntityId, Property):
    /// 1. IsCreate + IsDelete => Remove entirely (entity never synced)
    /// 2. IsCreate + IsUpdate => Keep as IsCreate with final values
    /// 3. IsUpdate + IsDelete => Keep as IsDelete with original OldValue
    /// </summary>
    private List<SyncChange> CollapsePatterns(IReadOnlyList<SyncChange> changes)
    {
        _logger?.LogDebug("Collapsing patterns in {Count} changes", changes.Count);

        var grouped = changes
            .GroupBy(c => new { c.EntityId, c.Property })
            .Select(g => CollapseGroupPatterns(g.OrderBy(c => c.Timestamp).ToList()))
            .Where(c => c != null)
            .ToList();

        var removedCount = changes.Count - grouped.Count;
        if (removedCount > 0)
        {
            _logger?.LogDebug("Removed {RemovedCount} changes through pattern collapse", removedCount);
        }

        return grouped!;
    }

    /// <summary>
    /// Collapse patterns within a single (EntityId, Property) group.
    /// </summary>
    private SyncChange? CollapseGroupPatterns(List<SyncChange> group)
    {
        if (group.Count == 1)
            return group[0];

        var first = group[0];
        var last = group[group.Count - 1];

        // Pattern: Create + Delete => Remove entirely
        if (first.IsCreate && last.IsDelete)
        {
            _logger?.LogDebug(
                "Collapsed Create+Delete pattern for {Entity}({EntityId}).{Property}",
                first.EntityType,
                first.EntityId,
                first.Property);
            return null;
        }

        // Pattern: Create + Update => Keep as Create with final values
        if (first.IsCreate && last.IsUpdate)
        {
            var result = new SyncChange(
                EntityId: first.EntityId,
                EntityType: first.EntityType,
                Property: first.Property,
                OldValue: first.OldValue,  // Keep Create's OldValue (null)
                NewValue: last.NewValue,    // Use Update's final NewValue
                Timestamp: last.Timestamp,
                UserId: last.UserId,
                ChangeId: first.ChangeId);

            _logger?.LogDebug(
                "Collapsed Create+Update pattern for {Entity}({EntityId}).{Property}",
                first.EntityType,
                first.EntityId,
                first.Property);
            return result;
        }

        // Pattern: Update + Delete => Keep as Delete with original OldValue
        if (first.IsUpdate && last.IsDelete)
        {
            var result = new SyncChange(
                EntityId: first.EntityId,
                EntityType: first.EntityType,
                Property: first.Property,
                OldValue: first.OldValue,   // Use Update's original OldValue
                NewValue: last.NewValue,     // Keep Delete's NewValue (null)
                Timestamp: last.Timestamp,
                UserId: last.UserId,
                ChangeId: first.ChangeId);

            _logger?.LogDebug(
                "Collapsed Update+Delete pattern for {Entity}({EntityId}).{Property}",
                first.EntityType,
                first.EntityId,
                first.Property);
            return result;
        }

        // No pattern match - merge OldValue from first and NewValue from last (standard dedup)
        return new SyncChange(
            EntityId: first.EntityId,
            EntityType: first.EntityType,
            Property: first.Property,
            OldValue: first.OldValue,
            NewValue: last.NewValue,
            Timestamp: last.Timestamp,
            UserId: last.UserId,
            ChangeId: first.ChangeId);
    }

    /// <summary>
    /// Estimate the JSON serialized size of changes.
    /// </summary>
    private long EstimateBatchSize(IReadOnlyList<SyncChange> changes)
    {
        try
        {
            var json = JsonSerializer.Serialize(changes);
            var baseSize = System.Text.Encoding.UTF8.GetByteCount(json);
            var withOverhead = (long)(baseSize * (1.0 + SizeEstimateOverheadPercent / 100.0));
            return withOverhead;
        }
        catch
        {
            // Fallback to rough estimate if serialization fails
            return (long)(changes.Count * 200 * (1.0 + SizeEstimateOverheadPercent / 100.0));
        }
    }
}
