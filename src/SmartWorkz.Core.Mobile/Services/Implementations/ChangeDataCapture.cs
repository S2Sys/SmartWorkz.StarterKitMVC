namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// In-memory Change Data Capture service for tracking entity changes during sync.
/// Thread-safe storage using ConcurrentBag with filtering and lifecycle management.
/// </summary>
public class ChangeDataCapture : IChangeDataCapture, IDisposable
{
    private readonly ConcurrentBag<SyncChange> _changes = new();
    private readonly ILogger? _logger;
    private bool _disposed;
    private readonly object _lockObject = new();

    public ChangeDataCapture(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Record a change to be tracked for sync.
    /// </summary>
    public Task<Result> RecordChangeAsync(SyncChange change)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(change, nameof(change));

        try
        {
            _changes.Add(change);
            _logger?.LogDebug(
                "Recorded change {ChangeId} for {Entity}({EntityId}): {Property}",
                change.ChangeId,
                change.EntityType,
                change.EntityId,
                change.Property);

            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to record change {ChangeId} for {Entity}({EntityId})",
                change.ChangeId,
                change.EntityType,
                change.EntityId);

            return Task.FromResult(
                Result.Fail("ChangeCapture.RecordFailed", "Failed to record change"));
        }
    }

    /// <summary>
    /// Get tracked changes with optional filtering.
    /// Returns results ordered by Timestamp DESC (most recent first).
    /// </summary>
    public Task<Result<IReadOnlyList<SyncChange>>> GetChangesAsync(
        string? entityType = null,
        DateTime? since = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            var changes = _changes.AsEnumerable();

            // Apply entityType filter (exact match, case-sensitive)
            if (!string.IsNullOrEmpty(entityType))
            {
                changes = changes.Where(c => c.EntityType == entityType);
                _logger?.LogDebug("Filtering changes by entityType: {EntityType}", entityType);
            }

            // Apply since filter (>= since timestamp)
            if (since.HasValue)
            {
                changes = changes.Where(c => c.Timestamp >= since.Value);
                _logger?.LogDebug("Filtering changes since: {Since:O}", since.Value);
            }

            // Order by Timestamp DESC (most recent first)
            IReadOnlyList<SyncChange> result = changes.OrderByDescending(c => c.Timestamp).ToList();

            _logger?.LogDebug(
                "Retrieved {Count} changes (entityType: {EntityType}, since: {Since:O})",
                result.Count,
                entityType ?? "all",
                since?.ToString("O") ?? "all");

            return Task.FromResult(Result.Ok(result));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to retrieve changes (entityType: {EntityType}, since: {Since:O})",
                entityType ?? "all",
                since?.ToString("O") ?? "all");

            return Task.FromResult(
                Result.Fail<IReadOnlyList<SyncChange>>(
                    "ChangeCapture.RetrieveFailed",
                    "Failed to retrieve changes"));
        }
    }

    /// <summary>
    /// Get a specific change by ID.
    /// </summary>
    public Task<Result<SyncChange>> GetChangeAsync(string changeId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotEmpty(changeId, nameof(changeId));

        try
        {
            var change = _changes.FirstOrDefault(c => c.ChangeId == changeId);
            if (change == null)
            {
                _logger?.LogWarning("Change not found: {ChangeId}", changeId);
                return Task.FromResult(
                    Result.Fail<SyncChange>("ChangeCapture.NotFound", $"Change {changeId} not found"));
            }

            _logger?.LogDebug("Retrieved change {ChangeId}", changeId);
            return Task.FromResult(Result.Ok(change));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to retrieve change {ChangeId}", changeId);
            return Task.FromResult(
                Result.Fail<SyncChange>(
                    "ChangeCapture.RetrieveFailed",
                    "Failed to retrieve change"));
        }
    }

    /// <summary>
    /// Clear tracked changes (optionally before a timestamp).
    /// If before is null, clears all changes.
    /// If before is provided, clears only changes BEFORE that timestamp.
    /// </summary>
    public Task<Result> ClearChangesAsync(DateTime? before = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            if (before == null)
            {
                // Clear all changes
                while (!_changes.IsEmpty)
                {
                    _changes.TryTake(out _);
                }

                _logger?.LogDebug("Cleared all changes");
                return Task.FromResult(Result.Ok());
            }

            // Clear only changes BEFORE the specified timestamp (atomically)
            lock (_lockObject)
            {
                var changesToKeep = _changes
                    .Where(c => c.Timestamp >= before.Value)
                    .ToList();

                // Rebuild the bag with only the changes to keep
                while (!_changes.IsEmpty)
                {
                    _changes.TryTake(out _);
                }

                foreach (var change in changesToKeep)
                {
                    _changes.Add(change);
                }

                _logger?.LogDebug(
                    "Cleared changes before {Before:O}, kept {KeptCount} changes",
                    before.Value,
                    changesToKeep.Count);
            }

            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to clear changes (before: {Before:O})",
                before?.ToString("O") ?? "all");

            return Task.FromResult(
                Result.Fail("ChangeCapture.ClearFailed", "Failed to clear changes"));
        }
    }

    /// <summary>
    /// Get total count of tracked changes.
    /// </summary>
    public Task<Result<int>> GetChangeCountAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            var count = _changes.Count;
            _logger?.LogDebug("Current change count: {Count}", count);
            return Task.FromResult(Result.Ok(count));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get change count");
            return Task.FromResult(
                Result.Fail<int>("ChangeCapture.CountFailed", "Failed to get change count"));
        }
    }

    /// <summary>
    /// Dispose and clean up resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            while (!_changes.IsEmpty)
            {
                _changes.TryTake(out _);
            }

            _logger?.LogDebug("ChangeDataCapture disposed");
        }
        finally
        {
            _disposed = true;
        }
    }
}
