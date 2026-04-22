namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;

public class AccelerometerServiceTests
{
    [Fact]
    public void IsMonitoring_DefaultFalse()
    {
        // Arrange & Act
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);

        // Assert
        Assert.False(service.IsMonitoring);
    }

    [Fact]
    public async Task StartMonitoringAsync_SetsIsMonitoringTrue()
    {
        // Arrange
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);

        // Act
        await service.StartMonitoringAsync(100);

        // Assert
        Assert.True(service.IsMonitoring);

        // Cleanup
        await service.StopMonitoringAsync();
    }

    [Fact]
    public async Task StopMonitoringAsync_SetsIsMonitoringFalse()
    {
        // Arrange
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);
        await service.StartMonitoringAsync(100);

        // Act
        await service.StopMonitoringAsync();

        // Assert
        Assert.False(service.IsMonitoring);
    }

    [Fact]
    public void OnReadingChanged_ReturnsObservable()
    {
        // Arrange
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);

        // Act
        var observable = service.OnReadingChanged();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public async Task StartMonitoringAsync_Idempotent_SecondCallDoesNothing()
    {
        // Arrange
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);

        // Act
        await service.StartMonitoringAsync(100);
        await service.StartMonitoringAsync(100); // Second call

        // Assert
        Assert.True(service.IsMonitoring);

        // Cleanup
        await service.StopMonitoringAsync();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Arrange
        var service = new AccelerometerService(NullLogger<AccelerometerService>.Instance);

        // Act
        var available = await service.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(available);
    }
}
