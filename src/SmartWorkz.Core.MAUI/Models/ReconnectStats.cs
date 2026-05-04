namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Statistics for auto-reconnection attempts and successes.
/// </summary>
public sealed record ReconnectStats(
    int TotalAttempts,
    int SuccessfulReconnects,
    DateTime LastReconnectTime,
    TimeSpan AvgReconnectDuration,
    int CurrentRetryCount);
