namespace SmartWorkz.Core.Shared.Diagnostics;

/// <summary>
/// Represents memory usage information.
/// </summary>
public class MemoryUsage
{
    /// <summary>
    /// Gets or sets the total memory available to the system (in bytes).
    /// </summary>
    public long TotalMemory { get; set; }

    /// <summary>
    /// Gets or sets the memory currently in use by the current process (in bytes).
    /// </summary>
    public long UsedMemory { get; set; }

    /// <summary>
    /// Gets or sets the memory available to allocate (in bytes).
    /// </summary>
    public long AvailableMemory { get; set; }

    /// <summary>
    /// Gets the percentage of memory in use by the current process.
    /// </summary>
    public double UsedPercentage => TotalMemory > 0 ? (double)UsedMemory / TotalMemory * 100 : 0;

    /// <summary>
    /// Gets the percentage of available memory relative to total.
    /// </summary>
    public double AvailablePercentage => TotalMemory > 0 ? (double)AvailableMemory / TotalMemory * 100 : 0;
}
