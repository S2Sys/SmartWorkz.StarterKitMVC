namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

#if !WINDOWS
public partial class SensorService : ISensorService
#else
public class SensorService : ISensorService
#endif
{
    private readonly ILogger _logger;

    public SensorService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public IAsyncEnumerable<SensorReading> StreamAccelerometerAsync(CancellationToken ct = default)
    {
        #if WINDOWS
        return AsyncEnumerable.Empty<SensorReading>();
        #else
        return StreamAccelerometerAsyncPlatform(ct);
        #endif
    }

    public IAsyncEnumerable<SensorReading> StreamGyroscopeAsync(CancellationToken ct = default)
    {
        #if WINDOWS
        return AsyncEnumerable.Empty<SensorReading>();
        #else
        return StreamGyroscopeAsyncPlatform(ct);
        #endif
    }

    public IAsyncEnumerable<SensorReading> StreamMagnetometerAsync(CancellationToken ct = default)
    {
        #if WINDOWS
        return AsyncEnumerable.Empty<SensorReading>();
        #else
        return StreamMagnetometerAsyncPlatform(ct);
        #endif
    }

    public async Task<DeviceOrientation> GetDeviceOrientationAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return DeviceOrientation.Unknown;
        #else
        try
        {
            return await GetDeviceOrientationAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device orientation");
            return DeviceOrientation.Unknown;
        }
        #endif
    }

    public IAsyncEnumerable<DeviceOrientation> StreamDeviceOrientationAsync(CancellationToken ct = default)
    {
        #if WINDOWS
        return AsyncEnumerable.Empty<DeviceOrientation>();
        #else
        return StreamDeviceOrientationAsyncPlatform(ct);
        #endif
    }

    public async Task<bool> IsSensorAvailableAsync(SensorType sensorType, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await IsSensorAvailableAsyncPlatform(sensorType, ct);
        }
        catch
        {
            return false;
        }
        #endif
    }

    public async Task<IEnumerable<SensorType>> GetAvailableSensorsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return Enumerable.Empty<SensorType>();
        #else
        try
        {
            return await GetAvailableSensorsAsyncPlatform(ct);
        }
        catch
        {
            return Enumerable.Empty<SensorType>();
        }
        #endif
    }

    #if !WINDOWS
    private partial IAsyncEnumerable<SensorReading> StreamAccelerometerAsyncPlatform(CancellationToken ct);
    private partial IAsyncEnumerable<SensorReading> StreamGyroscopeAsyncPlatform(CancellationToken ct);
    private partial IAsyncEnumerable<SensorReading> StreamMagnetometerAsyncPlatform(CancellationToken ct);
    private partial Task<DeviceOrientation> GetDeviceOrientationAsyncPlatform(CancellationToken ct);
    private partial IAsyncEnumerable<DeviceOrientation> StreamDeviceOrientationAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsSensorAvailableAsyncPlatform(SensorType sensorType, CancellationToken ct);
    private partial Task<IEnumerable<SensorType>> GetAvailableSensorsAsyncPlatform(CancellationToken ct);
    #endif
}
