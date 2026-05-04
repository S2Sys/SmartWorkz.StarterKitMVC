namespace SmartWorkz.Mobile;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

/// <summary>Service for accessing device accelerometer (motion sensor) data.</summary>
public sealed partial class AccelerometerService : IAccelerometerService
{
    private readonly ILogger<AccelerometerService> _logger;
    private readonly Subject<AccelerometerReading> _readings = new();
    private bool _isMonitoring;

    public bool IsMonitoring
    {
        get => _isMonitoring;
        private set => _isMonitoring = value;
    }

    public AccelerometerService(ILogger<AccelerometerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Checks if accelerometer hardware is available.</summary>
    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        IsAvailableAsyncPlatform(ct);

    /// <summary>Starts monitoring accelerometer with specified sample rate.</summary>
    public async Task StartMonitoringAsync(int? sampleRateMs = null, CancellationToken ct = default)
    {
        if (IsMonitoring) return;

        try
        {
            await StartMonitoringAsyncPlatform(sampleRateMs ?? 100, ct);
            IsMonitoring = true;
            _logger.LogInformation("Accelerometer monitoring started with {SampleRate}ms rate", sampleRateMs ?? 100);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start accelerometer monitoring");
            throw;
        }
    }

    /// <summary>Stops monitoring accelerometer readings.</summary>
    public async Task StopMonitoringAsync(CancellationToken ct = default)
    {
        if (!IsMonitoring) return;

        try
        {
            await StopMonitoringAsyncPlatform(ct);
            IsMonitoring = false;
            _logger.LogInformation("Accelerometer monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop accelerometer monitoring");
        }
    }

    /// <summary>Returns observable stream of accelerometer readings.</summary>
    public IObservable<AccelerometerReading> OnReadingChanged() => _readings.AsObservable();

    private void PublishReading(AccelerometerReading reading) =>
        _readings.OnNext(reading);

    // Platform-specific partial methods
#if __ANDROID__ || __IOS__
    private partial Task StartMonitoringAsyncPlatform(int sampleRateMs, CancellationToken ct);
    private partial Task StopMonitoringAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
#else
    private Task StartMonitoringAsyncPlatform(int sampleRateMs, CancellationToken ct) =>
        Task.CompletedTask;

    private Task StopMonitoringAsyncPlatform(CancellationToken ct) =>
        Task.CompletedTask;

    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);
#endif
}
