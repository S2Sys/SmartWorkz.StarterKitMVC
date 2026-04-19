namespace SmartWorkz.Core.Shared.Diagnostics;

/// <summary>
/// Represents CPU usage information.
/// </summary>
public class CpuUsage
{
    /// <summary>
    /// Gets or sets the CPU utilization percentage (0-100).
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Gets or sets the CPU load average (if available).
    /// </summary>
    public double LoadAverage { get; set; }
}
