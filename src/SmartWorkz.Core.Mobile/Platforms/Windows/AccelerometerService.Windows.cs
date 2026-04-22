#if __WINDOWS__
namespace SmartWorkz.Mobile;

/// <summary>Windows accelerometer implementation. Returns false (not available on Windows).</summary>
public sealed partial class AccelerometerService
{
    // Note: Windows.Devices.Sensors.Accelerometer could be used for WinUI apps
    // For now, this platform returns unavailable to match MAUI mobile-first design
}
#endif
