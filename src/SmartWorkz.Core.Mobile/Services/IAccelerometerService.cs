namespace SmartWorkz.Mobile;

/// <summary>Provides device accelerometer (motion sensor) access and streaming.</summary>
public interface IAccelerometerService
{
    /// <summary>Checks if accelerometer hardware is available on the device.</summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>Starts monitoring accelerometer readings.</summary>
    /// <param name="sampleRateMs">Optional sample rate in milliseconds (default: 100ms).</param>
    Task StartMonitoringAsync(int? sampleRateMs = null, CancellationToken ct = default);

    /// <summary>Stops monitoring accelerometer readings.</summary>
    Task StopMonitoringAsync(CancellationToken ct = default);

    /// <summary>Gets whether accelerometer is currently being monitored.</summary>
    bool IsMonitoring { get; }

    /// <summary>Returns an observable stream of accelerometer readings.</summary>
    IObservable<AccelerometerReading> OnReadingChanged();
}
