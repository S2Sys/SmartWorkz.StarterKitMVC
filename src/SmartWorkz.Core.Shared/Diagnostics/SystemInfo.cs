namespace SmartWorkz.Core.Shared.Diagnostics;

/// <summary>
/// Represents system information including CPU, memory, and disk details.
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// Gets or sets the CPU usage information.
    /// </summary>
    public CpuUsage Cpu { get; set; } = new();

    /// <summary>
    /// Gets or sets the memory usage information.
    /// </summary>
    public MemoryUsage Memory { get; set; } = new();

    /// <summary>
    /// Gets or sets the disk space information.
    /// </summary>
    public DiskSpace Disk { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of processors available on the system.
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// Gets the timestamp when this system information was gathered.
    /// </summary>
    public DateTime GatheredAt { get; set; } = DateTime.UtcNow;
}
