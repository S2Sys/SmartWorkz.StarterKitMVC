#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.Hardware;

public sealed partial class AccelerometerService : SensorEventListener
{
    private SensorManager? _sensorManager;
    private Sensor? _accelerometer;

    public void OnSensorChanged(SensorEvent? e)
    {
        if (e?.Values is null || e.Values.Length < 3) return;
        var reading = new AccelerometerReading(e.Values[0], e.Values[1], e.Values[2], DateTime.UtcNow);
        PublishReading(reading);
    }

    public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy) { }

    private partial async Task StartMonitoringAsyncPlatform(int sampleRateMs, CancellationToken ct)
    {
        var context = Android.App.Application.Context;
        _sensorManager = context?.GetSystemService(Android.App.Application.SensorService) as SensorManager;
        _accelerometer = _sensorManager?.GetDefaultSensor(SensorType.Accelerometer);

        if (_accelerometer is not null)
        {
            var delay = sampleRateMs switch
            {
                < 20 => SensorDelay.Fastest,
                < 50 => SensorDelay.Game,
                < 200 => SensorDelay.Ui,
                _ => SensorDelay.Normal
            };
            _sensorManager?.RegisterListener(this, _accelerometer, delay);
        }
        await Task.CompletedTask;
    }

    private partial async Task StopMonitoringAsyncPlatform(CancellationToken ct)
    {
        _sensorManager?.UnregisterListener(this, _accelerometer);
        await Task.CompletedTask;
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        var context = Android.App.Application.Context;
        var sensorManager = context?.GetSystemService(Android.App.Application.SensorService) as SensorManager;
        var accel = sensorManager?.GetDefaultSensor(SensorType.Accelerometer);
        return Task.FromResult(accel is not null);
    }
}
#endif
