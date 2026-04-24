namespace SmartWorkz.Mobile.Services;

using System;
using System.Threading.Tasks;
using SmartWorkz.Shared;

/// <summary>
/// Deduplication service to prevent processing duplicate messages.
/// </summary>
public interface IDeduplicationService
{
    /// <summary>
    /// Check if message is duplicate and record if new.
    /// </summary>
    /// <returns>false if duplicate, true if new message</returns>
    Task<Result<bool>> IsDuplicateAsync(string messageId);

    /// <summary>
    /// Record a message as seen (for manual tracking).
    /// </summary>
    Task<Result> RecordMessageAsync(string messageId);

    /// <summary>
    /// Clear old deduplication records.
    /// </summary>
    Task<Result> CleanupAsync(TimeSpan olderThan);

    /// <summary>
    /// Get count of tracked messages.
    /// </summary>
    Task<Result<int>> GetTrackedCountAsync();
}
