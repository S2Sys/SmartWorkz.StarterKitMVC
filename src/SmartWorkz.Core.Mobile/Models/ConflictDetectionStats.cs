namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Statistics about conflict detection sessions.
/// Tracks metrics for monitoring conflict detection performance and frequency.
/// </summary>
public sealed record ConflictDetectionStats(
    int TotalDetectionRuns,
    int ConflictsFound,
    int EntitiesInvolved,
    DateTime LastDetectionTime,
    int AverageConflictsPerRun)
{
    /// <summary>
    /// Display summary for logging and monitoring.
    /// </summary>
    public string DisplaySummary =>
        $"{TotalDetectionRuns} runs, {ConflictsFound} total conflicts, avg {AverageConflictsPerRun}/run";
}
