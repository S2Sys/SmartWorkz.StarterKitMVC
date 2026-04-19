using System.Diagnostics;
using System.IO;

namespace SmartWorkz.Core.Shared.Diagnostics;

/// <summary>
/// Sealed helper class for system diagnostics and application health monitoring.
/// Provides methods to gather system information, CPU/memory/disk usage, and determine application health.
/// </summary>
public sealed class DiagnosticsHelper
{
    private static DateTime _applicationStartTime = DateTime.UtcNow;
    private static PerformanceCounter? _cpuCounter;
    private static readonly object _cpuLock = new();

    /// <summary>
    /// Memory warning threshold: 80% usage.
    /// </summary>
    private const double MemoryWarningThreshold = 80.0;

    /// <summary>
    /// Memory critical threshold: 95% usage.
    /// </summary>
    private const double MemoryCriticalThreshold = 95.0;

    /// <summary>
    /// Disk warning threshold: 85% usage.
    /// </summary>
    private const double DiskWarningThreshold = 85.0;

    /// <summary>
    /// Disk critical threshold: 95% usage.
    /// </summary>
    private const double DiskCriticalThreshold = 95.0;

    // Prevent instantiation - sealed class with private constructor
    private DiagnosticsHelper()
    {
    }

    /// <summary>
    /// Initializes the application start time (called once at application startup).
    /// </summary>
    public static void Initialize()
    {
        _applicationStartTime = DateTime.UtcNow;
        InitializeCpuCounter();
    }

    /// <summary>
    /// Gets comprehensive system information including CPU, memory, disk, and processor count.
    /// </summary>
    /// <returns>A Result containing SystemInfo or error details.</returns>
    public static Result<SystemInfo> GetSystemInfo()
    {
        try
        {
            var systemInfo = new SystemInfo
            {
                Cpu = GetCpuUsageInternal(),
                Memory = GetMemoryUsageInternal(),
                Disk = GetDiskSpaceInternal("C:"),
                ProcessorCount = Environment.ProcessorCount,
                GatheredAt = DateTime.UtcNow
            };

            return Result.Ok(systemInfo);
        }
        catch (Exception ex)
        {
            return Result.Fail<SystemInfo>(
                "Error.SystemInfo",
                $"Failed to retrieve system information: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Gets memory usage statistics for the current process and system.
    /// </summary>
    /// <returns>A Result containing MemoryUsage or error details.</returns>
    public static Result<MemoryUsage> GetMemoryUsage()
    {
        try
        {
            var memUsage = GetMemoryUsageInternal();
            return Result.Ok(memUsage);
        }
        catch (Exception ex)
        {
            return Result.Fail<MemoryUsage>(
                "Error.MemoryUsage",
                $"Failed to retrieve memory usage: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Gets CPU utilization percentage.
    /// </summary>
    /// <returns>A Result containing CpuUsage or error details.</returns>
    public static Result<CpuUsage> GetCpuUsage()
    {
        try
        {
            var cpuUsage = GetCpuUsageInternal();
            return Result.Ok(cpuUsage);
        }
        catch (Exception ex)
        {
            return Result.Fail<CpuUsage>(
                "Error.CpuUsage",
                $"Failed to retrieve CPU usage: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Gets disk space information for a specific drive.
    /// </summary>
    /// <param name="drive">The drive letter (e.g., "C:", "D:"). Defaults to "C:".</param>
    /// <returns>A Result containing DiskSpace or error details.</returns>
    public static Result<DiskSpace> GetDiskSpace(string drive = "C:")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(drive))
            {
                drive = "C:";
            }

            var diskSpace = GetDiskSpaceInternal(drive);
            return Result.Ok(diskSpace);
        }
        catch (Exception ex)
        {
            return Result.Fail<DiskSpace>(
                "Error.DiskSpace",
                $"Failed to retrieve disk space for drive {drive}: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Gets the application uptime since the last Initialize() call or application start.
    /// </summary>
    /// <returns>A Result containing the uptime as a TimeSpan or error details.</returns>
    public static Result<TimeSpan> GetUptime()
    {
        try
        {
            var uptime = DateTime.UtcNow - _applicationStartTime;
            return Result.Ok(uptime);
        }
        catch (Exception ex)
        {
            return Result.Fail<TimeSpan>(
                "Error.Uptime",
                $"Failed to calculate uptime: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Gets the overall health status of the application based on system metrics.
    /// </summary>
    /// <returns>A Result containing ApplicationHealth or error details.</returns>
    public static Result<ApplicationHealth> GetApplicationHealth()
    {
        try
        {
            var health = new ApplicationHealth
            {
                AssessmentTime = DateTime.UtcNow,
                HealthChecks = new()
            };

            // Memory check
            var memResult = GetMemoryUsage();
            if (memResult.Succeeded && memResult.Data != null)
            {
                var memCheck = new HealthCheck
                {
                    Name = "Memory",
                    Timestamp = DateTime.UtcNow,
                    Message = $"Memory usage: {memResult.Data.UsedPercentage:F2}%"
                };

                if (memResult.Data.UsedPercentage >= MemoryCriticalThreshold)
                {
                    memCheck.Status = HealthStatus.Critical;
                }
                else if (memResult.Data.UsedPercentage >= MemoryWarningThreshold)
                {
                    memCheck.Status = HealthStatus.Warning;
                }
                else
                {
                    memCheck.Status = HealthStatus.Healthy;
                }

                health.HealthChecks.Add(memCheck);
            }

            // Disk check
            var diskResult = GetDiskSpace("C:");
            if (diskResult.Succeeded && diskResult.Data != null)
            {
                var diskCheck = new HealthCheck
                {
                    Name = "Disk",
                    Timestamp = DateTime.UtcNow,
                    Message = $"Disk usage: {diskResult.Data.UsedPercentage:F2}%"
                };

                if (diskResult.Data.UsedPercentage >= DiskCriticalThreshold)
                {
                    diskCheck.Status = HealthStatus.Critical;
                }
                else if (diskResult.Data.UsedPercentage >= DiskWarningThreshold)
                {
                    diskCheck.Status = HealthStatus.Warning;
                }
                else
                {
                    diskCheck.Status = HealthStatus.Healthy;
                }

                health.HealthChecks.Add(diskCheck);
            }

            // Determine overall status
            if (health.HealthChecks.Any(h => h.Status == HealthStatus.Critical))
            {
                health.Status = HealthStatus.Critical;
            }
            else if (health.HealthChecks.Any(h => h.Status == HealthStatus.Warning))
            {
                health.Status = HealthStatus.Warning;
            }
            else
            {
                health.Status = HealthStatus.Healthy;
            }

            return Result.Ok(health);
        }
        catch (Exception ex)
        {
            return Result.Fail<ApplicationHealth>(
                "Error.ApplicationHealth",
                $"Failed to assess application health: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// Determines if the application is considered healthy based on the provided health status.
    /// </summary>
    /// <param name="health">The ApplicationHealth object to evaluate.</param>
    /// <returns>True if the status is Healthy, false otherwise.</returns>
    public static bool IsHealthy(ApplicationHealth health)
    {
        return health?.Status == HealthStatus.Healthy;
    }

    /// <summary>
    /// Initializes the CPU performance counter (called once).
    /// </summary>
    private static void InitializeCpuCounter()
    {
        try
        {
            lock (_cpuLock)
            {
                if (_cpuCounter == null)
                {
                    _cpuCounter = new PerformanceCounter(
                        "Processor",
                        "% Processor Time",
                        "_Total"
                    );
                    // Initial read to warm up the counter
                    _ = _cpuCounter.NextValue();
                }
            }
        }
        catch
        {
            // CPU counter may not be available on all systems - gracefully handle
        }
    }

    /// <summary>
    /// Internal method to get memory usage statistics.
    /// </summary>
    private static MemoryUsage GetMemoryUsageInternal()
    {
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;

        // Get GC heap size as total memory for the process
        var memInfo = GC.GetGCMemoryInfo();
        var heapSize = memInfo.HeapSizeBytes;

        // Use the larger of working set or heap size as the effective total
        var totalMemory = Math.Max(workingSet, heapSize);
        var availableMemory = Math.Max(0, totalMemory - workingSet);

        return new MemoryUsage
        {
            TotalMemory = totalMemory,
            UsedMemory = workingSet,
            AvailableMemory = availableMemory
        };
    }

    /// <summary>
    /// Internal method to get CPU usage percentage.
    /// </summary>
    private static CpuUsage GetCpuUsageInternal()
    {
        double percentage = 0;

        try
        {
            lock (_cpuLock)
            {
                if (_cpuCounter != null)
                {
                    percentage = _cpuCounter.NextValue();
                }
            }
        }
        catch
        {
            // CPU counter may fail - return 0
            percentage = 0;
        }

        // Clamp to 0-100 range
        percentage = Math.Max(0, Math.Min(100, percentage));

        return new CpuUsage
        {
            Percentage = percentage,
            LoadAverage = percentage // LoadAverage approximated as current percentage
        };
    }

    /// <summary>
    /// Internal method to get disk space information.
    /// </summary>
    private static DiskSpace GetDiskSpaceInternal(string drive)
    {
        var driveInfo = new DriveInfo(drive);

        if (!driveInfo.IsReady)
        {
            return new DiskSpace
            {
                DriveLabel = drive,
                TotalSpace = 0,
                FreeSpace = 0,
                UsedSpace = 0
            };
        }

        var totalSpace = driveInfo.TotalSize;
        var freeSpace = driveInfo.AvailableFreeSpace;
        var usedSpace = totalSpace - freeSpace;

        return new DiskSpace
        {
            DriveLabel = driveInfo.VolumeLabel ?? drive,
            TotalSpace = totalSpace,
            FreeSpace = freeSpace,
            UsedSpace = usedSpace
        };
    }
}
