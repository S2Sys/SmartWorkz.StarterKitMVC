namespace SmartWorkz.Shared;

/// <summary>
/// Represents the overall health status of the application.
/// </summary>
public class ApplicationHealth
{
    /// <summary>
    /// Gets or sets the overall health status.
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the list of individual health checks performed.
    /// </summary>
    public List<HealthCheck> HealthChecks { get; set; } = new();

    /// <summary>
    /// Gets the timestamp when this health assessment was performed.
    /// </summary>
    public DateTime AssessmentTime { get; set; } = DateTime.UtcNow;
}
