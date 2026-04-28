namespace SmartWorkz.Mobile.Tests.Mocks;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;

/// <summary>
/// Mock location provider for testing without requiring device/simulator access.
/// Simulates location updates with realistic coordinate changes.
/// </summary>
public class MockLocationProvider : ILocationService
{
    private readonly Location[] _predefinedLocations = new[]
    {
        new Location { Latitude = 40.7128, Longitude = -74.0060, Accuracy = 10, Timestamp = DateTime.UtcNow }, // NYC
        new Location { Latitude = 40.7130, Longitude = -74.0062, Accuracy = 12, Timestamp = DateTime.UtcNow.AddSeconds(1) }, // NYC nearby
        new Location { Latitude = 34.0522, Longitude = -118.2437, Accuracy = 15, Timestamp = DateTime.UtcNow.AddSeconds(2) }, // LA
    };

    private int _currentLocationIndex = 0;
    private PermissionStatus _permissionStatus = PermissionStatus.WhenInUse;
    private bool _locationEnabled = true;

    /// <summary>
    /// Sets the mock permission status for testing permission scenarios.
    /// </summary>
    public void SetPermissionStatus(PermissionStatus status) => _permissionStatus = status;

    /// <summary>
    /// Sets whether location services are enabled/disabled.
    /// </summary>
    public void SetLocationEnabled(bool enabled) => _locationEnabled = enabled;

    public Task<Location> GetCurrentLocationAsync()
    {
        if (!_locationEnabled)
            throw new InvalidOperationException("Location services are disabled");

        if (_permissionStatus == PermissionStatus.Denied)
            throw new UnauthorizedAccessException("Location permission denied");

        var location = _predefinedLocations[_currentLocationIndex % _predefinedLocations.Length];
        _currentLocationIndex++;

        return Task.FromResult(location);
    }

    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (_permissionStatus == PermissionStatus.Denied)
            throw new UnauthorizedAccessException("Location permission denied");

        int index = 0;
        while (!ct.IsCancellationRequested && index < _predefinedLocations.Length)
        {
            yield return _predefinedLocations[index];
            await Task.Delay(100, ct);
            index++;
        }
    }

    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        var history = _predefinedLocations
            .Where(loc => loc.Timestamp >= startDate && loc.Timestamp <= endDate)
            .ToList();

        return Task.FromResult(history);
    }

    public Task StartBackgroundTrackingAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopBackgroundTrackingAsync()
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsLocationEnabledAsync()
    {
        return Task.FromResult(_locationEnabled);
    }

    public Task<PermissionStatus> GetPermissionStatusAsync()
    {
        return Task.FromResult(_permissionStatus);
    }
}
