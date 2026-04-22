namespace SmartWorkz.Mobile;

#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using SmartWorkz.Shared;
using System.Threading;
using System.Threading.Tasks;

partial class GeofencingService
{
    private LocationManager? _locationManager;
    private GeofenceProximityAlertReceiver? _proximityAlertReceiver;

    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(GeofenceRegion region, CancellationToken ct)
    {
        try
        {
            var context = Microsoft.Maui.Controls.Application.Current?.MainPage?.Handler?.MauiContext?.Context;
            if (context == null)
            {
                return Result.Fail<bool>(Error.NotFound("LOCATION.CONTEXT_NOT_FOUND",
                    "Android context unavailable"));
            }

            _locationManager = context.GetSystemService(Context.LocationService) as LocationManager;
            if (_locationManager == null)
            {
                return Result.Fail<bool>(Error.NotFound("LOCATION.SERVICE_NOT_FOUND",
                    "LocationManager service unavailable"));
            }

            if (!_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                return Result.Fail<bool>(Error.Validation("LOCATION.GPS_DISABLED",
                    "GPS location provider is disabled"));
            }

            var pendingIntent = CreateProximityAlertIntent(context, region);
            _locationManager.AddProximityAlert(
                region.Latitude,
                region.Longitude,
                (float)region.RadiusMeters,
                -1, // No expiration
                pendingIntent);

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android geofence start failed");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(string regionId, CancellationToken ct)
    {
        try
        {
            var context = Microsoft.Maui.Controls.Application.Current?.MainPage?.Handler?.MauiContext?.Context;
            if (context != null && _locationManager != null)
            {
                var pendingIntent = CreateProximityAlertIntent(context, null);
                _locationManager.RemoveProximityAlert(pendingIntent);
            }

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android geofence stop failed");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        var context = Microsoft.Maui.Controls.Application.Current?.MainPage?.Handler?.MauiContext?.Context;
        if (context == null) return false;

        var locationManager = context.GetSystemService(Context.LocationService) as LocationManager;
        return await Task.FromResult(locationManager?.IsProviderEnabled(LocationManager.GpsProvider) ?? false);
    }

    private PendingIntent? CreateProximityAlertIntent(Context context, GeofenceRegion? region)
    {
        var intent = new Intent(context, typeof(GeofenceProximityAlertReceiver));
        if (region != null)
        {
            intent.PutExtra("region_id", region.Id);
            intent.PutExtra("region_name", region.Name);
        }

        return PendingIntent.GetBroadcast(context, region?.Id.GetHashCode() ?? 0,
            intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
    }
}

/// <summary>
/// Broadcast receiver for proximity alert events on Android.
/// </summary>
[BroadcastReceiver(Exported = false)]
public class GeofenceProximityAlertReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var regionId = intent.GetStringExtra("region_id") ?? "";
        var isEnteringKey = LocationManager.KeyProximityEntering;
        var entering = intent.GetBooleanExtra(isEnteringKey, false);
    }
}
#endif
