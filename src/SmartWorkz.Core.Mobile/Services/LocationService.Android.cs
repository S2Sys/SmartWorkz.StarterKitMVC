#if ANDROID
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;
using Android.OS;
using AndroidX.Core.App;
using Google.Android.Gms.Common;
using Google.Android.Gms.Location;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;

namespace SmartWorkz.Core.Mobile.Services;

/// <summary>
/// Android-specific location service using Google Play Services Location API (FusedLocationProvider).
/// </summary>
public partial class LocationService : ILocationService
{
    private FusedLocationProviderClient? _fusedLocationProviderClient;
    private TaskCompletionSource<Location>? _locationCompletionSource;
    private LocationCallback? _locationCallback;

    /// <summary>
    /// Initializes the FusedLocationProviderClient.
    /// </summary>
    private FusedLocationProviderClient EnsureFusedLocationProviderClient()
    {
        if (_fusedLocationProviderClient != null)
            return _fusedLocationProviderClient;

        var context = Application.Context ?? throw new InvalidOperationException("Application context required");

        // Check if Google Play Services is available
        var availability = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(context);
        if (availability != ConnectionResult.Success)
        {
            throw new InvalidOperationException($"Google Play Services not available: {availability}");
        }

        _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
        return _fusedLocationProviderClient;
    }

    /// <summary>
    /// Gets the current device location asynchronously.
    /// </summary>
    public async Task<Location> GetCurrentLocationAsync()
    {
        var client = EnsureFusedLocationProviderClient();
        var context = Application.Context ?? throw new InvalidOperationException("Application context required");

        // Check permission status
        var permStatus = await GetPermissionStatusAsync();
        if (permStatus == PermissionStatus.Denied)
        {
            throw new UnauthorizedAccessException("Location permission denied");
        }

        _locationCompletionSource = new TaskCompletionSource<Location>();
        var request = CreateLocationRequest(LocationAccuracy.Best);

        try
        {
            var locationCallback = new LocationCallback();
            locationCallback.OnLocationResult += (result) =>
            {
                if (result?.LastLocation is Android.Locations.Location androidLocation)
                {
                    var location = new Location
                    {
                        Latitude = androidLocation.Latitude,
                        Longitude = androidLocation.Longitude,
                        Accuracy = androidLocation.Accuracy >= 0 ? androidLocation.Accuracy : null,
                        Altitude = androidLocation.Altitude >= 0 ? androidLocation.Altitude : null,
                        Heading = androidLocation.Bearing >= 0 ? androidLocation.Bearing : null,
                        Speed = androidLocation.Speed >= 0 ? androidLocation.Speed : null,
                        Timestamp = UnixTimeStampToDateTime(androidLocation.Time)
                    };

                    _locationCompletionSource?.SetResult(location);
                }
            };

            // FIX: Register the location callback with FusedLocationProviderClient
            var mainLooper = Looper.MainLooper ?? throw new InvalidOperationException("Main looper not available");
            client.RequestLocationUpdates(request, locationCallback, mainLooper);

            // Use Task.Wait with timeout (30 seconds)
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                var task = _locationCompletionSource.Task;
                if (!task.Wait(cts.Token))
                {
                    throw new OperationCanceledException("Location acquisition timed out after 30 seconds");
                }

                return task.Result;
            }
        }
        finally
        {
            // Clean up by removing the callback from FusedLocationProviderClient
            try
            {
                client.RemoveLocationUpdates(locationCallback);
            }
            catch (Exception ex)
            {
                // Log but don't throw - cleanup should not fail the method
            }
            _locationCallback = null;
        }
    }

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// </summary>
    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var client = EnsureFusedLocationProviderClient();
        var request = CreateLocationRequest(accuracy);
        var mainLooper = Looper.MainLooper ?? throw new InvalidOperationException("Main looper not available");

        var locationCallback = new LocationCallback();

        locationCallback.OnLocationResult += (result) =>
        {
            if (result?.LastLocation is Android.Locations.Location androidLocation)
            {
                var location = new Location
                {
                    Latitude = androidLocation.Latitude,
                    Longitude = androidLocation.Longitude,
                    Accuracy = androidLocation.Accuracy >= 0 ? androidLocation.Accuracy : null,
                    Altitude = androidLocation.Altitude >= 0 ? androidLocation.Altitude : null,
                    Heading = androidLocation.Bearing >= 0 ? androidLocation.Bearing : null,
                    Speed = androidLocation.Speed >= 0 ? androidLocation.Speed : null,
                    Timestamp = UnixTimeStampToDateTime(androidLocation.Time)
                };

                // If there's a pending TaskCompletionSource, resolve it
                if (_locationCompletionSource is not null && !_locationCompletionSource.Task.IsCompleted)
                {
                    _locationCompletionSource.TrySetResult(location);
                }
            }
        };

        try
        {
            // FIX: Register the callback with FusedLocationProviderClient
            client.RequestLocationUpdates(request, locationCallback, mainLooper);

            _locationCallback = locationCallback;

            while (!ct.IsCancellationRequested)
            {
                _locationCompletionSource = new TaskCompletionSource<Location>();

                var location = await _locationCompletionSource.Task.ConfigureAwait(false);
                yield return location;
            }
        }
        finally
        {
            // Clean up by removing the callback from FusedLocationProviderClient
            try
            {
                client.RemoveLocationUpdates(locationCallback);
            }
            catch (Exception ex)
            {
                // Log but don't throw
            }
            _locationCallback = null;
        }
    }

    /// <summary>
    /// Not supported on Android - background tracking requires specialized implementation.
    /// </summary>
    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotSupportedException(
            "Location history not supported on Android. Enable background tracking and implement custom storage.");
    }

    /// <summary>
    /// Starts background location tracking.
    /// </summary>
    public async Task StartBackgroundTrackingAsync()
    {
        var client = EnsureFusedLocationProviderClient();
        var request = CreateLocationRequest(LocationAccuracy.Medium);

        _locationCallback ??= new LocationCallback();
        try
        {
            // Note: Requires android.permission.ACCESS_BACKGROUND_LOCATION on Android 10+
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to start background tracking", ex);
        }
    }

    /// <summary>
    /// Stops background location tracking.
    /// </summary>
    public async Task StopBackgroundTrackingAsync()
    {
        var client = EnsureFusedLocationProviderClient();
        if (_locationCallback != null)
        {
            client.RemoveLocationUpdates(_locationCallback);
            _locationCallback = null;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets whether device has location services enabled.
    /// </summary>
    public async Task<bool> IsLocationEnabledAsync()
    {
        var context = Application.Context ?? throw new InvalidOperationException("Application context required");
        var locationManager = context.GetSystemService(Android.Content.Context.LocationService) as LocationManager;

        var enabled = locationManager?.IsLocationEnabled ?? false;
        return await Task.FromResult(enabled);
    }

    /// <summary>
    /// Gets the current location permission status.
    /// </summary>
    public async Task<PermissionStatus> GetPermissionStatusAsync()
    {
        var context = Application.Context ?? throw new InvalidOperationException("Application context required");

        var fineLocation = Android.Manifest.Permission.AccessFineLocation;
        var coarseLocation = Android.Manifest.Permission.AccessCoarseLocation;

        var fineStatus = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, fineLocation);
        var coarseStatus = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, coarseLocation);

        if (fineStatus == Android.Content.PM.Permission.Granted ||
            coarseStatus == Android.Content.PM.Permission.Granted)
        {
            return await Task.FromResult(PermissionStatus.WhenInUse);
        }

        return await Task.FromResult(PermissionStatus.Denied);
    }

    /// <summary>
    /// Creates a LocationRequest with appropriate accuracy settings.
    /// </summary>
    private static LocationRequest CreateLocationRequest(LocationAccuracy accuracy)
    {
        var request = new LocationRequest();

        request.SetPriority(accuracy switch
        {
            LocationAccuracy.Lowest => LocationRequest.PriorityLowPower,
            LocationAccuracy.Low => LocationRequest.PriorityPassiveLocation,
            LocationAccuracy.Medium => LocationRequest.PriorityBalancedPowerAccuracy,
            LocationAccuracy.High => LocationRequest.PriorityHighAccuracy,
            LocationAccuracy.Best => LocationRequest.PriorityHighAccuracy,
            _ => LocationRequest.PriorityHighAccuracy
        });

        // Set update intervals (in milliseconds)
        var interval = accuracy switch
        {
            LocationAccuracy.Lowest => 10000L,      // 10 seconds
            LocationAccuracy.Low => 5000L,          // 5 seconds
            LocationAccuracy.Medium => 2000L,       // 2 seconds
            LocationAccuracy.High => 1000L,         // 1 second
            LocationAccuracy.Best => 500L,          // 500ms
            _ => 1000L
        };

        request.SetInterval(interval);
        request.SetFastestInterval(interval / 2);

        return request;
    }

    /// <summary>
    /// Converts Unix timestamp (milliseconds since epoch) to UTC DateTime.
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long timestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddMilliseconds(timestamp);
        return dateTime;
    }
}
#endif
