#if WINDOWS
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;

namespace SmartWorkz.Core.Mobile.Services;

/// <summary>
/// Windows-specific location service using UWP Geolocator API.
/// </summary>
public partial class LocationService : ILocationService
{
    private Geolocator? _geolocator;
    private TaskCompletionSource<Location>? _locationCompletionSource;

    /// <summary>
    /// Initializes the Geolocator.
    /// </summary>
    private Geolocator EnsureGeolocator()
    {
        if (_geolocator != null)
            return _geolocator;

        _geolocator = new Geolocator();
        _geolocator.DesiredAccuracy = PositionAccuracy.High;

        return _geolocator;
    }

    /// <summary>
    /// Gets the current device location asynchronously.
    /// </summary>
    public async Task<Location> GetCurrentLocationAsync()
    {
        var geolocator = EnsureGeolocator();

        try
        {
            // Set timeout for position acquisition (30 seconds)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                var position = await geolocator.GetGeopositionAsync().AsTask(cts.Token);

                if (position?.Coordinate == null)
                    throw new InvalidOperationException("Failed to acquire location");

                return new Location
                {
                    Latitude = position.Coordinate.Point.Position.Latitude,
                    Longitude = position.Coordinate.Point.Position.Longitude,
                    Accuracy = position.Coordinate.Accuracy >= 0 ? position.Coordinate.Accuracy : null,
                    Altitude = position.Coordinate.Point.Position.Altitude >= 0 ? position.Coordinate.Point.Position.Altitude : null,
                    AltitudeAccuracy = position.Coordinate.AltitudeAccuracy >= 0 ? position.Coordinate.AltitudeAccuracy : null,
                    Heading = position.Coordinate.Heading >= 0 ? position.Coordinate.Heading : null,
                    Speed = position.Coordinate.Speed >= 0 ? position.Coordinate.Speed : null,
                    Timestamp = position.Coordinate.Timestamp.ToUniversalTime()
                };
            }
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Location acquisition timed out after 30 seconds");
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Location permission denied");
        }
    }

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// </summary>
    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var geolocator = EnsureGeolocator();
        SetAccuracy(geolocator, accuracy);

        // Create handler for position changes
        void PositionChangedHandler(Geolocator sender, PositionChangedEventArgs args)
        {
            if (args.Position?.Coordinate != null)
            {
                _locationCompletionSource?.TrySetResult(ConvertToLocation(args.Position));
            }
        }

        try
        {
            geolocator.PositionChanged += PositionChangedHandler;

            while (!ct.IsCancellationRequested)
            {
                _locationCompletionSource = new TaskCompletionSource<Location>();

                var location = await _locationCompletionSource.Task.ConfigureAwait(false);
                yield return location;
            }
        }
        finally
        {
            geolocator.PositionChanged -= PositionChangedHandler;
        }
    }

    /// <summary>
    /// Not supported on Windows - location history requires custom implementation.
    /// </summary>
    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotSupportedException(
            "Location history not supported on Windows. Enable background tracking and implement custom storage.");
    }

    /// <summary>
    /// Starts background location tracking.
    /// </summary>
    public async Task StartBackgroundTrackingAsync()
    {
        var geolocator = EnsureGeolocator();

        // Windows requires a foreground service or app window to track location
        // This is a simplified implementation
        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops background location tracking.
    /// </summary>
    public async Task StopBackgroundTrackingAsync()
    {
        var geolocator = EnsureGeolocator();
        // Background tracking cleanup if implemented
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets whether device has location services enabled.
    /// </summary>
    public async Task<bool> IsLocationEnabledAsync()
    {
        try
        {
            var geolocator = EnsureGeolocator();
            // Try to get a location - if it fails, services are disabled
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await geolocator.GetGeopositionAsync().AsTask(cts.Token);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current location permission status.
    /// </summary>
    public async Task<PermissionStatus> GetPermissionStatusAsync()
    {
        try
        {
            var geolocator = EnsureGeolocator();

            // Windows uses app capability declarations - check if we can access location
            // If we reach here without exception, we have permission
            await GetCurrentLocationAsync();
            return PermissionStatus.WhenInUse;
        }
        catch (UnauthorizedAccessException)
        {
            return PermissionStatus.Denied;
        }
        catch
        {
            return PermissionStatus.NotRequested;
        }
    }

    /// <summary>
    /// Sets accuracy level on the Geolocator.
    /// </summary>
    private static void SetAccuracy(Geolocator geolocator, LocationAccuracy accuracy)
    {
        geolocator.DesiredAccuracy = accuracy switch
        {
            LocationAccuracy.Lowest => PositionAccuracy.Default,
            LocationAccuracy.Low => PositionAccuracy.Default,
            LocationAccuracy.Medium => PositionAccuracy.Default,
            LocationAccuracy.High => PositionAccuracy.High,
            LocationAccuracy.Best => PositionAccuracy.High,
            _ => PositionAccuracy.High
        };
    }

    /// <summary>
    /// Converts a Windows Geoposition to our Location model.
    /// </summary>
    private static Location ConvertToLocation(Geoposition geoposition)
    {
        if (geoposition?.Coordinate == null)
            throw new ArgumentNullException(nameof(geoposition));

        return new Location
        {
            Latitude = geoposition.Coordinate.Point.Position.Latitude,
            Longitude = geoposition.Coordinate.Point.Position.Longitude,
            Accuracy = geoposition.Coordinate.Accuracy >= 0 ? geoposition.Coordinate.Accuracy : null,
            Altitude = geoposition.Coordinate.Point.Position.Altitude >= 0 ? geoposition.Coordinate.Point.Position.Altitude : null,
            AltitudeAccuracy = geoposition.Coordinate.AltitudeAccuracy >= 0 ? geoposition.Coordinate.AltitudeAccuracy : null,
            Heading = geoposition.Coordinate.Heading >= 0 ? geoposition.Coordinate.Heading : null,
            Speed = geoposition.Coordinate.Speed >= 0 ? geoposition.Coordinate.Speed : null,
            Timestamp = geoposition.Coordinate.Timestamp.ToUniversalTime()
        };
    }
}
#endif
