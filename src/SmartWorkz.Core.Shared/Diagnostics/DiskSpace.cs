namespace SmartWorkz.Shared;

/// <summary>
/// Represents disk space information for a drive.
/// </summary>
public class DiskSpace
{
    /// <summary>
    /// Gets or sets the total disk space (in bytes).
    /// </summary>
    public long TotalSpace { get; set; }

    /// <summary>
    /// Gets or sets the free disk space available (in bytes).
    /// </summary>
    public long FreeSpace { get; set; }

    /// <summary>
    /// Gets or sets the used disk space (in bytes).
    /// </summary>
    public long UsedSpace { get; set; }

    /// <summary>
    /// Gets or sets the drive label/name.
    /// </summary>
    public string DriveLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets the percentage of disk space in use.
    /// </summary>
    public double UsedPercentage => TotalSpace > 0 ? (double)UsedSpace / TotalSpace * 100 : 0;

    /// <summary>
    /// Gets the percentage of free disk space.
    /// </summary>
    public double FreePercentage => TotalSpace > 0 ? (double)FreeSpace / TotalSpace * 100 : 0;
}
