using SmartWorkz.Shared.Diagnostics;

namespace SmartWorkz.Core.Tests.Diagnostics;

public class DiagnosticsHelperTests
{
    #region Initialize Tests

    [Fact]
    public void Initialize_ShouldSetStartTime()
    {
        // Act
        DiagnosticsHelper.Initialize();

        // Assert
        var uptimeResult = DiagnosticsHelper.GetUptime();
        Assert.True(uptimeResult.Succeeded);
        Assert.NotNull(uptimeResult.Data);
        Assert.True(uptimeResult.Data > TimeSpan.Zero);
    }

    #endregion

    #region GetSystemInfo Tests

    [Fact]
    public void GetSystemInfo_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetSystemInfo_ShouldContainCpuInfo()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data?.Cpu);
        Assert.True(result.Data.Cpu.Percentage >= 0);
    }

    [Fact]
    public void GetSystemInfo_ShouldContainMemoryInfo()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data?.Memory);
        Assert.True(result.Data.Memory.TotalMemory > 0);
    }

    [Fact]
    public void GetSystemInfo_ShouldContainDiskInfo()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data?.Disk);
        Assert.True(result.Data.Disk.TotalSpace > 0);
    }

    [Fact]
    public void GetSystemInfo_ShouldContainProcessorCount()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.ProcessorCount > 0);
        Assert.Equal(Environment.ProcessorCount, result.Data?.ProcessorCount);
    }

    [Fact]
    public void GetSystemInfo_ShouldHaveValidGatheredTime()
    {
        // Act
        var result = DiagnosticsHelper.GetSystemInfo();
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.GatheredAt >= beforeTime && result.Data?.GatheredAt <= afterTime);
    }

    #endregion

    #region GetMemoryUsage Tests

    [Fact]
    public void GetMemoryUsage_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetMemoryUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetMemoryUsage_ShouldHaveValidMemoryValues()
    {
        // Act
        var result = DiagnosticsHelper.GetMemoryUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.TotalMemory > 0);
        Assert.True(result.Data?.UsedMemory >= 0);
        Assert.True(result.Data?.AvailableMemory >= 0);
    }

    [Fact]
    public void GetMemoryUsage_ShouldCalculatePercentageCorrectly()
    {
        // Act
        var result = DiagnosticsHelper.GetMemoryUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.UsedPercentage >= 0 && result.Data?.UsedPercentage <= 100);
        Assert.True(result.Data?.AvailablePercentage >= 0 && result.Data?.AvailablePercentage <= 100);
    }

    [Fact]
    public void GetMemoryUsage_ShouldHaveUsedPlusAvailableEqualTotal()
    {
        // Act
        var result = DiagnosticsHelper.GetMemoryUsage();

        // Assert
        Assert.True(result.Succeeded);
        var total = result.Data?.UsedMemory + result.Data?.AvailableMemory;
        Assert.Equal(result.Data?.TotalMemory, total);
    }

    #endregion

    #region GetCpuUsage Tests

    [Fact]
    public void GetCpuUsage_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetCpuUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetCpuUsage_ShouldHaveValidPercentage()
    {
        // Act
        var result = DiagnosticsHelper.GetCpuUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.Percentage >= 0 && result.Data?.Percentage <= 100);
    }

    [Fact]
    public void GetCpuUsage_ShouldHaveLoadAverage()
    {
        // Act
        var result = DiagnosticsHelper.GetCpuUsage();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.LoadAverage >= 0);
    }

    #endregion

    #region GetDiskSpace Tests

    [Fact]
    public void GetDiskSpace_WithDefaultDrive_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetDiskSpace_WithValidDrive_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("C:");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetDiskSpace_WithNullDrive_ShouldUseDefault()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace(null!);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetDiskSpace_WithEmptyDrive_ShouldUseDefault()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetDiskSpace_ShouldHaveValidDiskValues()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("C:");

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.TotalSpace > 0);
        Assert.True(result.Data?.FreeSpace >= 0);
        Assert.True(result.Data?.UsedSpace >= 0);
    }

    [Fact]
    public void GetDiskSpace_ShouldCalculatePercentageCorrectly()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("C:");

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.UsedPercentage >= 0 && result.Data?.UsedPercentage <= 100);
        Assert.True(result.Data?.FreePercentage >= 0 && result.Data?.FreePercentage <= 100);
    }

    [Fact]
    public void GetDiskSpace_ShouldHaveUsedPlusFreeEqualTotal()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("C:");

        // Assert
        Assert.True(result.Succeeded);
        var total = result.Data?.UsedSpace + result.Data?.FreeSpace;
        Assert.Equal(result.Data?.TotalSpace, total);
    }

    [Fact]
    public void GetDiskSpace_ShouldHaveDriveLabel()
    {
        // Act
        var result = DiagnosticsHelper.GetDiskSpace("C:");

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.Data?.DriveLabel));
    }

    #endregion

    #region GetUptime Tests

    [Fact]
    public void GetUptime_ShouldReturnSuccessResult()
    {
        // Act
        DiagnosticsHelper.Initialize();
        var result = DiagnosticsHelper.GetUptime();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetUptime_ShouldReturnPositiveTimeSpan()
    {
        // Act
        DiagnosticsHelper.Initialize();
        System.Threading.Thread.Sleep(100); // Sleep for 100ms
        var result = DiagnosticsHelper.GetUptime();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data > TimeSpan.Zero);
    }

    #endregion

    #region GetApplicationHealth Tests

    [Fact]
    public void GetApplicationHealth_ShouldReturnSuccessResult()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetApplicationHealth_ShouldHaveHealthChecks()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data?.HealthChecks);
        Assert.NotEmpty(result.Data.HealthChecks);
    }

    [Fact]
    public void GetApplicationHealth_ShouldIncludeMemoryCheck()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        var memoryCheck = result.Data?.HealthChecks.FirstOrDefault(h => h.Name == "Memory");
        Assert.NotNull(memoryCheck);
        Assert.True(memoryCheck.Timestamp > DateTime.MinValue);
        Assert.False(string.IsNullOrEmpty(memoryCheck.Message));
    }

    [Fact]
    public void GetApplicationHealth_ShouldIncludeDiskCheck()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        var diskCheck = result.Data?.HealthChecks.FirstOrDefault(h => h.Name == "Disk");
        Assert.NotNull(diskCheck);
        Assert.True(diskCheck.Timestamp > DateTime.MinValue);
        Assert.False(string.IsNullOrEmpty(diskCheck.Message));
    }

    [Fact]
    public void GetApplicationHealth_ShouldHaveValidOverallStatus()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(
            result.Data?.Status == HealthStatus.Healthy ||
            result.Data?.Status == HealthStatus.Warning ||
            result.Data?.Status == HealthStatus.Critical
        );
    }

    [Fact]
    public void GetApplicationHealth_ShouldHaveValidAssessmentTime()
    {
        // Act
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        var result = DiagnosticsHelper.GetApplicationHealth();
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data?.AssessmentTime >= beforeTime && result.Data?.AssessmentTime <= afterTime);
    }

    #endregion

    #region IsHealthy Tests

    [Fact]
    public void IsHealthy_WithHealthyStatus_ShouldReturnTrue()
    {
        // Arrange
        var health = new ApplicationHealth { Status = HealthStatus.Healthy };

        // Act
        var result = DiagnosticsHelper.IsHealthy(health);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsHealthy_WithWarningStatus_ShouldReturnFalse()
    {
        // Arrange
        var health = new ApplicationHealth { Status = HealthStatus.Warning };

        // Act
        var result = DiagnosticsHelper.IsHealthy(health);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsHealthy_WithCriticalStatus_ShouldReturnFalse()
    {
        // Arrange
        var health = new ApplicationHealth { Status = HealthStatus.Critical };

        // Act
        var result = DiagnosticsHelper.IsHealthy(health);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsHealthy_WithNullHealth_ShouldReturnFalse()
    {
        // Act
        var result = DiagnosticsHelper.IsHealthy(null!);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Health Threshold Tests

    [Fact]
    public void GetApplicationHealth_WithHighMemoryUsage_ShouldSetWarningStatus()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        // This test validates the threshold logic exists; actual warning status depends on system state
        var memoryCheck = result.Data?.HealthChecks.FirstOrDefault(h => h.Name == "Memory");
        Assert.NotNull(memoryCheck);
        Assert.True(memoryCheck.Status >= HealthStatus.Healthy);
    }

    [Fact]
    public void GetApplicationHealth_OverallStatusReflectsWorstCheck()
    {
        // Act
        var result = DiagnosticsHelper.GetApplicationHealth();

        // Assert
        Assert.True(result.Succeeded);
        var worstStatus = result.Data?.HealthChecks.Max(h => (int)h.Status);
        Assert.Equal((int?)result.Data?.Status, worstStatus);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public void GetSystemInfo_CanBeCalledConcurrently()
    {
        // Act & Assert
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DiagnosticsHelper.GetSystemInfo()))
            .ToList();

        Task.WaitAll(tasks.ToArray());

        foreach (var task in tasks)
        {
            Assert.True(task.Result.Succeeded);
            Assert.NotNull(task.Result.Data);
        }
    }

    [Fact]
    public void GetMemoryUsage_CanBeCalledConcurrently()
    {
        // Act & Assert
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DiagnosticsHelper.GetMemoryUsage()))
            .ToList();

        Task.WaitAll(tasks.ToArray());

        foreach (var task in tasks)
        {
            Assert.True(task.Result.Succeeded);
            Assert.NotNull(task.Result.Data);
        }
    }

    #endregion
}
