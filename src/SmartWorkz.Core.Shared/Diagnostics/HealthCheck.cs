namespace SmartWorkz.Shared;

/// <summary>
/// Represents a single health check result.
/// </summary>
public class HealthCheck
{
    /// <summary>
    /// Gets or sets the name of the health check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of this health check.
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message about the health check result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the health check was performed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
