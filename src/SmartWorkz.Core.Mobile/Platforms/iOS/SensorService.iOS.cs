namespace SmartWorkz.Mobile;

#if __IOS__

public partial class SensorService
{
    private partial IAsyncEnumerable<SensorReading> StreamAccelerometerAsyncPlatform(CancellationToken ct) => AsyncEnumerable.Empty<SensorReading>();
    private partial IAsyncEnumerable<SensorReading> StreamGyroscopeAsyncPlatform(CancellationToken ct) => AsyncEnumerable.Empty<SensorReading>();
    private partial IAsyncEnumerable<SensorReading> StreamMagnetometerAsyncPlatform(CancellationToken ct) => AsyncEnumerable.Empty<SensorReading>();
    private partial async Task<DeviceOrientation> GetDeviceOrientationAsyncPlatform(CancellationToken ct) => DeviceOrientation.Portrait;
    private partial IAsyncEnumerable<DeviceOrientation> StreamDeviceOrientationAsyncPlatform(CancellationToken ct) => AsyncEnumerable.Empty<DeviceOrientation>();
    private partial async Task<bool> IsSensorAvailableAsyncPlatform(SensorType sensorType, CancellationToken ct) => true;
    private partial async Task<IEnumerable<SensorType>> GetAvailableSensorsAsyncPlatform(CancellationToken ct) => new[] { SensorType.Accelerometer, SensorType.Gyroscope, SensorType.Magnetometer };
}

#endif
