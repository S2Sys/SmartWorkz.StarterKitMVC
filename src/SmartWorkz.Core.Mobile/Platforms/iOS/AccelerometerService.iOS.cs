#if __IOS__
namespace SmartWorkz.Mobile;

using CoreMotion;

public sealed partial class AccelerometerService
{
    private CMMotionManager? _motionManager;

    private partial async Task StartMonitoringAsyncPlatform(int sampleRateMs, CancellationToken ct)
    {
        _motionManager = new CMMotionManager();
        if (!_motionManager.AccelerometerAvailable) return;

        _motionManager.AccelerometerUpdateInterval = sampleRateMs / 1000.0;
        var queue = new NSOperationQueue();
        _motionManager.StartAccelerometerUpdates(queue, (data, error) =>
        {
            if (data?.Acceleration is not null)
            {
                var reading = new AccelerometerReading(
                    data.Acceleration.X, data.Acceleration.Y, data.Acceleration.Z, DateTime.UtcNow);
                PublishReading(reading);
            }
        });
        await Task.CompletedTask;
    }

    private partial async Task StopMonitoringAsyncPlatform(CancellationToken ct)
    {
        _motionManager?.StopAccelerometerUpdates();
        _motionManager?.Dispose();
        _motionManager = null;
        await Task.CompletedTask;
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        var manager = new CMMotionManager();
        return Task.FromResult(manager.AccelerometerAvailable);
    }
}
#endif
