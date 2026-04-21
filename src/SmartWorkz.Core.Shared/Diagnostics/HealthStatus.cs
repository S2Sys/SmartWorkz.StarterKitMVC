namespace SmartWorkz.Shared;

/// <summary>
/// Represents the health status of the application.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Application is healthy and operating normally.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Application is operating but with warnings (approaching thresholds).
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Application is in critical condition (exceeded thresholds).
    /// </summary>
    Critical = 2
}
