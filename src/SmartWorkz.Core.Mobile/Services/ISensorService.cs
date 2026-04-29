namespace SmartWorkz.Mobile;

/// <summary>
/// Service for accessing device sensors (accelerometer, gyroscope, compass, etc.).
/// Provides real-time sensor data streaming and one-time readings.
/// </summary>
public interface ISensorService
{
    /// <summary>
    /// Streams accelerometer data (X, Y, Z acceleration values).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Async enumeration of accelerometer readings</returns>
    IAsyncEnumerable<SensorReading> StreamAccelerometerAsync(CancellationToken ct = default);

    /// <summary>
    /// Streams gyroscope data (rotation rate around X, Y, Z axes).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Async enumeration of gyroscope readings</returns>
    IAsyncEnumerable<SensorReading> StreamGyroscopeAsync(CancellationToken ct = default);

    /// <summary>
    /// Streams magnetometer/compass data (X, Y, Z magnetic field values).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Async enumeration of magnetometer readings</returns>
    IAsyncEnumerable<SensorReading> StreamMagnetometerAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the current device orientation (portrait, landscape, etc.).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current device orientation</returns>
    Task<DeviceOrientation> GetDeviceOrientationAsync(CancellationToken ct = default);

    /// <summary>
    /// Streams device orientation changes.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Async enumeration of orientation changes</returns>
    IAsyncEnumerable<DeviceOrientation> StreamDeviceOrientationAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if a specific sensor is available on the device.
    /// </summary>
    /// <param name="sensorType">Type of sensor to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if sensor is available</returns>
    Task<bool> IsSensorAvailableAsync(SensorType sensorType, CancellationToken ct = default);

    /// <summary>
    /// Gets all available sensors on the device.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of available sensor types</returns>
    Task<IEnumerable<SensorType>> GetAvailableSensorsAsync(CancellationToken ct = default);
}

/// <summary>
/// Represents a reading from a sensor.
/// </summary>
public class SensorReading
{
    /// <summary>Gets or sets the X-axis value.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y-axis value.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the Z-axis value.</summary>
    public double Z { get; set; }

    /// <summary>Gets or sets the timestamp of the reading.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets or sets the sensor accuracy/precision.</summary>
    public SensorAccuracy Accuracy { get; set; }
}

/// <summary>
/// Types of sensors available on mobile devices.
/// </summary>
public enum SensorType
{
    /// <summary>Accelerometer sensor</summary>
    Accelerometer = 0,

    /// <summary>Gyroscope sensor</summary>
    Gyroscope = 1,

    /// <summary>Magnetometer/Compass sensor</summary>
    Magnetometer = 2,

    /// <summary>Barometer sensor</summary>
    Barometer = 3,

    /// <summary>Ambient light sensor</summary>
    Light = 4,

    /// <summary>Proximity sensor</summary>
    Proximity = 5,
}

/// <summary>
/// Device orientation values.
/// </summary>
public enum DeviceOrientation
{
    /// <summary>Unknown orientation</summary>
    Unknown = 0,

    /// <summary>Portrait orientation (top at top)</summary>
    Portrait = 1,

    /// <summary>Landscape orientation (left at top)</summary>
    Landscape = 2,

    /// <summary>Reverse portrait orientation (bottom at top)</summary>
    ReversePortrait = 3,

    /// <summary>Reverse landscape orientation (right at top)</summary>
    ReverseLandscape = 4,
}

/// <summary>
/// Sensor reading accuracy levels.
/// </summary>
public enum SensorAccuracy
{
    /// <summary>Low accuracy</summary>
    Low = 0,

    /// <summary>Medium accuracy</summary>
    Medium = 1,

    /// <summary>High accuracy</summary>
    High = 2,
}
