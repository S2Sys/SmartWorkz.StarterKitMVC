# SmartWorkz.Core.Mobile - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete mobile platform services ecosystem by implementing 9 missing critical services (LocationService, CameraService, BiometricService, FilePickerService, PermissionService, NotificationService, AudioService, CalendarService, SensorService) with 100% cross-platform support (iOS, Android, Windows, macOS).

**Architecture:** 
- Platform-specific implementation pattern following existing ContactsService model
- Shared interface + platform-specific implementations
- Permission handling per platform requirements
- Consistent API across all platforms
- MAUI 8.0+ native platform support

**Tech Stack:** .NET MAUI, .NET 9, iOS/Android/Windows/macOS native APIs, xUnit, Platform-specific SDKs (CoreLocation, Google Play Services, Windows Runtime)

**Timeline:** 8-10 weeks, 3-4 developers (mobile expertise required)  
**Effort:** ~160 developer days  
**Priority:** CRITICAL - Blocks mobile app development

---

## Current State vs Target

### Current
- 1 platform service (ContactsService) with iOS/Android/Windows/macOS implementations
- 7 total files
- 0 unit tests
- 0 documentation files
- 100% XML documentation (limited to existing service)

### Target
- 10 platform services fully implemented
- 4 platform implementations per service (iOS, Android, Windows, macOS)
- 50+ unit tests (covering platform-agnostic logic)
- 10+ integration tests (per-platform tests)
- Complete documentation (README, API reference, platform guides)
- 100% XML documentation on all services

---

## Phase 1: Foundation & LocationService (2 weeks)

### Task 1: Create Location Models and Interface

**Files:**
- Create: `src/SmartWorkz.Core.Mobile/Models/Location.cs`
- Create: `src/SmartWorkz.Core.Mobile/Models/LocationAccuracy.cs`
- Create: `src/SmartWorkz.Core.Mobile/Services/ILocationService.cs`
- Create: `tests/LocationServiceTests.cs`

- [ ] **Step 1: Define location models**

```csharp
// src/SmartWorkz.Core.Mobile/Models/Location.cs
namespace SmartWorkz.Core.Mobile.Models;

/// <summary>
/// Represents a geographic location with coordinates, accuracy, and timestamp.
/// </summary>
public class Location : IEquatable<Location>
{
    /// <summary>Gets or sets latitude in degrees (-90 to +90).</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets longitude in degrees (-180 to +180).</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets horizontal accuracy in meters (null if unknown).</summary>
    public double? Accuracy { get; set; }

    /// <summary>Gets or sets altitude in meters above sea level (null if unknown).</summary>
    public double? Altitude { get; set; }

    /// <summary>Gets or sets vertical accuracy in meters (null if unknown).</summary>
    public double? AltitudeAccuracy { get; set; }

    /// <summary>Gets or sets heading in degrees (0-360, null if unknown).</summary>
    public double? Heading { get; set; }

    /// <summary>Gets or sets speed in meters per second (null if unknown).</summary>
    public double? Speed { get; set; }

    /// <summary>Gets or sets UTC timestamp when location was acquired.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets the distance in meters between this location and another.</summary>
    public double GetDistanceTo(Location other)
    {
        const double earthRadiusMeters = 6371000;
        
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLng = ToRadians(other.Longitude - Longitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public bool Equals(Location? other) => 
        other != null && 
        Latitude == other.Latitude && 
        Longitude == other.Longitude;

    public override bool Equals(object? obj) => Equals(obj as Location);
    public override int GetHashCode() => (Latitude, Longitude).GetHashCode();
}

// src/SmartWorkz.Core.Mobile/Models/LocationAccuracy.cs
namespace SmartWorkz.Core.Mobile.Models;

/// <summary>
/// Defines location accuracy levels balancing power consumption and precision.
/// </summary>
public enum LocationAccuracy
{
    /// <summary>Lowest accuracy, best battery life (~5000m error).</summary>
    Lowest = 0,

    /// <summary>Low accuracy (~500m error).</summary>
    Low = 1,

    /// <summary>Medium accuracy (~100m error).</summary>
    Medium = 2,

    /// <summary>High accuracy (~10m error).</summary>
    High = 3,

    /// <summary>Highest accuracy (~1m error), worst battery life.</summary>
    Best = 4
}
```

- [ ] **Step 2: Define ILocationService interface**

```csharp
// src/SmartWorkz.Core.Mobile/Services/ILocationService.cs
namespace SmartWorkz.Core.Mobile.Services;

using SmartWorkz.Core.Mobile.Models;

/// <summary>
/// Provides unified access to platform-specific location services (GPS, geolocation).
/// Supports single location retrieval, continuous monitoring, and location history.
/// </summary>
/// <remarks>
/// Implementation varies by platform:
/// - iOS: Uses CoreLocation.CLLocationManager
/// - Android: Uses Google Play Services Location API
/// - Windows: Uses Geolocator from Windows Runtime
/// - macOS: Uses CoreLocation.CLLocationManager
/// 
/// Always request permissions before calling location methods.
/// Battery usage increases significantly with higher accuracy levels and continuous monitoring.
/// </remarks>
public interface ILocationService
{
    /// <summary>
    /// Gets the current device location asynchronously.
    /// </summary>
    /// <returns>Location with latitude, longitude, and metadata.</returns>
    /// <exception cref="OperationCanceledException">If location retrieval times out.</exception>
    /// <remarks>
    /// First call may take several seconds while GPS acquires signal lock.
    /// Subsequent calls may return cached location if within time threshold.
    /// Respects accuracy settings and may retry internally.
    /// </remarks>
    Task<Location> GetCurrentLocationAsync();

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// </summary>
    /// <param name="accuracy">Desired accuracy level (affects battery usage).</param>
    /// <returns>Async enumerable that yields locations as they are acquired.</returns>
    /// <remarks>
    /// This method does not complete on its own; continue enumerating until manually cancelled.
    /// Usage:
    /// <code>
    /// await foreach (var location in locationService.WatchLocationAsync())
    /// {
    ///     Console.WriteLine($"Location: {location.Latitude}, {location.Longitude}");
    /// }
    /// </code>
    /// </remarks>
    IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best);

    /// <summary>
    /// Retrieves location history for a date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive).</param>
    /// <param name="endDate">End date (inclusive).</param>
    /// <returns>List of locations recorded within date range.</returns>
    /// <remarks>Availability depends on whether background tracking is enabled.</remarks>
    Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Starts background location tracking for offline synchronization.
    /// </summary>
    /// <remarks>
    /// Requires 'Always' permission level on iOS and backgroundLocation permission on Android.
    /// Enable based on use case; background tracking consumes significant battery.
    /// </remarks>
    Task StartBackgroundTrackingAsync();

    /// <summary>
    /// Stops background location tracking.
    /// </summary>
    Task StopBackgroundTrackingAsync();

    /// <summary>
    /// Gets whether device has location services enabled.
    /// </summary>
    Task<bool> IsLocationEnabledAsync();

    /// <summary>
    /// Gets the current location permission status.
    /// </summary>
    Task<PermissionStatus> GetPermissionStatusAsync();
}

/// <summary>Permission status for location access.</summary>
public enum PermissionStatus
{
    /// <summary>Permission not yet requested.</summary>
    NotRequested = 0,

    /// <summary>Permission denied by user or system policy.</summary>
    Denied = 1,

    /// <summary>Permission granted only when app is in use.</summary>
    WhenInUse = 2,

    /// <summary>Permission granted always, including background.</summary>
    Always = 3
}
```

- [ ] **Step 3: Write failing tests**

```csharp
// tests/LocationServiceTests.cs
namespace SmartWorkz.Core.Mobile.Tests;

using SmartWorkz.Core.Mobile.Models;
using SmartWorkz.Core.Mobile.Services;

[TestFixture]
public class LocationServiceTests
{
    [Test]
    public void Location_GetDistanceTo_CalculatesCorrectly()
    {
        var location1 = new Location { Latitude = 40.7128, Longitude = -74.0060 }; // NYC
        var location2 = new Location { Latitude = 34.0522, Longitude = -118.2437 }; // LA

        var distance = location1.GetDistanceTo(location2);

        // NYC to LA is approximately 3944 km
        Assert.IsTrue(distance > 3900000 && distance < 4000000, 
            $"Expected distance around 3944km, got {distance / 1000}km");
    }

    [Test]
    public void Location_Equals_WorksCorrectly()
    {
        var loc1 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc2 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc3 = new Location { Latitude = 34.0522, Longitude = -118.2437 };

        Assert.AreEqual(loc1, loc2);
        Assert.AreNotEqual(loc1, loc3);
    }

    [Test]
    public void Location_WithNullAccuracy_IsValid()
    {
        var location = new Location 
        { 
            Latitude = 40.7128, 
            Longitude = -74.0060,
            Accuracy = null 
        };

        Assert.IsNull(location.Accuracy);
        Assert.IsNotNull(location);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Mobile
dotnet test tests/LocationServiceTests.cs -v
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Core.Mobile/Models/ src/SmartWorkz.Core.Mobile/Services/ILocationService.cs tests/LocationServiceTests.cs
git commit -m "feat(mobile): add Location models and ILocationService interface"
```

---

### Task 2: Implement LocationService.iOS

**Files:**
- Create: `src/SmartWorkz.Core.Mobile/Services/LocationService.iOS.cs`
- Create: `tests/LocationService.iOS.Tests.cs`

- [ ] **Step 1: Write failing test for iOS implementation**

```csharp
[TestFixture]
public class LocationServiceiOSTests
{
    [Test]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        if (!OperatingSystem.IsIOS())
            Assert.Inconclusive("iOS-specific test");

        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.IsNotNull(location);
        Assert.IsTrue(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.IsTrue(location.Longitude >= -180 && location.Longitude <= 180);
    }
}
```

- [ ] **Step 2: Implement iOS LocationService**

```csharp
// src/SmartWorkz.Core.Mobile/Services/LocationService.iOS.cs
#if IOS
using CoreLocation;
using Foundation;
using SmartWorkz.Core.Mobile.Models;

namespace SmartWorkz.Core.Mobile.Services;

/// <summary>
/// iOS-specific location service using CoreLocation framework.
/// </summary>
public partial class LocationService : ILocationService
{
    private CLLocationManager? _locationManager;
    private TaskCompletionSource<Location>? _locationCompletionSource;

    private CLLocationManager EnsureLocationManager()
    {
        if (_locationManager != null)
            return _locationManager;

        _locationManager = new CLLocationManager();
        _locationManager.WeakDelegate = new LocationDelegate(this);
        
        return _locationManager;
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        var manager = EnsureLocationManager();
        
        var status = CLLocationManager.Status;
        if (status == CLAuthorizationStatus.Denied || 
            status == CLAuthorizationStatus.Restricted)
            throw new UnauthorizedAccessException("Location access denied");

        if (status == CLAuthorizationStatus.NotDetermined)
            manager.RequestWhenInUseAuthorization();

        _locationCompletionSource = new TaskCompletionSource<Location>();
        manager.StartUpdatingLocation();

        var location = await _locationCompletionSource.Task.ConfigureAwait(false);
        manager.StopUpdatingLocation();

        return location;
    }

    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best)
    {
        var manager = EnsureLocationManager();
        SetAccuracy(manager, accuracy);
        
        manager.StartUpdatingLocation();

        while (true)
        {
            var tcs = new TaskCompletionSource<Location>();
            _locationCompletionSource = tcs;
            
            var location = await tcs.Task.ConfigureAwait(false);
            yield return location;
        }
    }

    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        // iOS: Not natively supported; would require custom implementation
        throw new NotSupportedException("Location history not supported on iOS");
    }

    public Task StartBackgroundTrackingAsync()
    {
        EnsureLocationManager().StartUpdatingLocation();
        return Task.CompletedTask;
    }

    public Task StopBackgroundTrackingAsync()
    {
        EnsureLocationManager().StopUpdatingLocation();
        return Task.CompletedTask;
    }

    public Task<bool> IsLocationEnabledAsync()
    {
        var enabled = CLLocationManager.LocationServicesEnabled;
        return Task.FromResult(enabled);
    }

    public async Task<PermissionStatus> GetPermissionStatusAsync()
    {
        var status = CLLocationManager.Status;
        return status switch
        {
            CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Always,
            CLAuthorizationStatus.AuthorizedWhenInUse => PermissionStatus.WhenInUse,
            CLAuthorizationStatus.Denied => PermissionStatus.Denied,
            _ => PermissionStatus.NotRequested
        };
    }

    private static void SetAccuracy(CLLocationManager manager, LocationAccuracy accuracy)
    {
        manager.DesiredAccuracy = accuracy switch
        {
            LocationAccuracy.Lowest => CLLocation.AccuracyThreeKilometers,
            LocationAccuracy.Low => CLLocation.AccuracyKilometer,
            LocationAccuracy.Medium => CLLocation.AccuracyHundredMeters,
            LocationAccuracy.High => CLLocation.AccuracyTenMeters,
            LocationAccuracy.Best => CLLocation.AccuracyBest,
            _ => CLLocation.AccuracyBest
        };
    }

    private class LocationDelegate : CLLocationManagerDelegate
    {
        private readonly LocationService _service;

        public LocationDelegate(LocationService service) => _service = service;

        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation? oldLocation)
        {
            var location = new Location
            {
                Latitude = newLocation.Coordinate.Latitude,
                Longitude = newLocation.Coordinate.Longitude,
                Accuracy = newLocation.HorizontalAccuracy >= 0 ? newLocation.HorizontalAccuracy : null,
                Altitude = newLocation.Altitude >= 0 ? newLocation.Altitude : null,
                AltitudeAccuracy = newLocation.VerticalAccuracy >= 0 ? newLocation.VerticalAccuracy : null,
                Timestamp = DateTime.UtcNow
            };

            _service._locationCompletionSource?.SetResult(location);
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            _service._locationCompletionSource?.SetException(
                new Exception($"Location failed: {error.LocalizedDescription}"));
        }
    }
}
#endif
```

- [ ] **Step 3: Run iOS tests**

```bash
# Only runs on iOS device/simulator
dotnet test tests/LocationService.iOS.Tests.cs -v
```

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.Mobile/Services/LocationService.iOS.cs tests/LocationService.iOS.Tests.cs
git commit -m "feat(mobile): implement LocationService for iOS using CoreLocation"
```

---

### Task 3-6: Implement LocationService for Android, Windows, macOS

Following the same pattern as iOS implementation, create platform-specific implementations:

**Task 3:** `LocationService.Android.cs` (3-4 days)
- Use Google Play Services Location API
- Handle permission requests
- Implement FusedLocationProviderClient
- LocationRequest with accuracy levels

**Task 4:** `LocationService.Windows.cs` (2-3 days)
- Use Windows Runtime Geolocator
- Handle UWP permission model
- Simpler than Android (fewer permission requests)

**Task 5:** `LocationService.macCatalyst.cs` (2 days)
- Reuse iOS implementation with macOS adjustments
- CoreLocation framework compatibility

**Task 6:** Integration tests and permission handling (2-3 days)
- Cross-platform permission abstractions
- Mock location provider for tests
- Permission request UI framework

**Total for LocationService:** 2-3 weeks, 2 developers

---

## Phase 2: CameraService (1-2 weeks)

### Task 7: CameraService Interface & Models

**Files:**
- Create: `src/SmartWorkz.Core.Mobile/Models/Photo.cs`
- Create: `src/SmartWorkz.Core.Mobile/Models/Video.cs`
- Create: `src/SmartWorkz.Core.Mobile/Services/ICameraService.cs`

```csharp
// src/SmartWorkz.Core.Mobile/Models/Photo.cs
namespace SmartWorkz.Core.Mobile.Models;

/// <summary>Represents a captured or selected photo.</summary>
public class Photo
{
    /// <summary>Gets or sets the file path where photo is saved.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets raw image bytes.</summary>
    public byte[]? ImageBytes { get; set; }

    /// <summary>Gets or sets the Base64-encoded image for transmission.</summary>
    public string? Base64 => ImageBytes != null ? Convert.ToBase64String(ImageBytes) : null;

    /// <summary>Gets or sets width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets height in pixels.</summary>
    public int Height { get; set; }

    /// <summary>Gets or sets capture timestamp.</summary>
    public DateTime CaptureTime { get; set; } = DateTime.UtcNow;
}

/// <summary>Represents a captured or selected video.</summary>
public class Video
{
    /// <summary>Gets or sets the file path where video is saved.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the video duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Gets or sets video dimensions (WIDTHxHEIGHT).</summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>Gets or sets capture timestamp.</summary>
    public DateTime CaptureTime { get; set; } = DateTime.UtcNow;
}

// src/SmartWorkz.Core.Mobile/Services/ICameraService.cs
namespace SmartWorkz.Core.Mobile.Services;

using SmartWorkz.Core.Mobile.Models;

/// <summary>
/// Provides unified access to device camera for photo capture, video recording, and media picking.
/// Supports taking photos/videos and selecting from device photo library.
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Captures a single photo from device camera.
    /// </summary>
    /// <returns>Photo with image data and metadata.</returns>
    /// <exception cref="OperationCanceledException">If user cancels camera.</exception>
    Task<Photo> TakePhotoAsync();

    /// <summary>
    /// Records video from device camera with optional duration limit.
    /// </summary>
    /// <param name="maxDuration">Maximum recording duration (null for unlimited).</param>
    /// <returns>Video with file path and metadata.</returns>
    Task<Video> RecordVideoAsync(TimeSpan? maxDuration = null);

    /// <summary>
    /// Opens photo library to select multiple photos.
    /// </summary>
    /// <returns>List of selected photos.</returns>
    Task<List<Photo>> PickMultiplePhotosAsync();

    /// <summary>
    /// Opens photo library to select single photo.
    /// </summary>
    /// <returns>Selected photo or null if cancelled.</returns>
    Task<Photo?> PickSinglePhotoAsync();

    /// <summary>
    /// Gets whether device camera is available.
    /// </summary>
    Task<bool> IsCameraAvailableAsync();

    /// <summary>
    /// Gets camera permission status.
    /// </summary>
    Task<PermissionStatus> GetCameraPermissionAsync();

    /// <summary>
    /// Gets photo library permission status.
    /// </summary>
    Task<PermissionStatus> GetPhotoLibraryPermissionAsync();
}
```

### Task 8-11: Implement CameraService for All Platforms

- **Task 8:** iOS implementation using AVFoundation (3-4 days)
- **Task 9:** Android implementation using Camera2/CameraX API (3-4 days)
- **Task 10:** Windows implementation (2-3 days)
- **Task 11:** Tests and permission handling (2-3 days)

**Total for CameraService:** 2 weeks, 2 developers

---

## Phase 3: Additional Services (3-4 weeks)

Implement remaining 7 services following same pattern:

### BiometricService (3-4 days each platform)
- iOS: LocalAuthentication framework (Face ID, Touch ID)
- Android: BiometricPrompt API
- Windows: Windows.Security.Credentials
- Uses MAUI Essentials pattern

### FilePickerService (2-3 days each platform)
- File selection UI for documents, media
- Multi-file selection support
- File type filtering

### PermissionService (3-4 days)
- Unified permission request/status API
- Platform-specific permission mappings
- Request chaining (multiple permissions)

### NotificationService (2-3 days)
- Local push notifications
- Notification scheduling
- Custom sound/vibration

### AudioService (2-3 days)
- Audio playback
- Recording
- Audio effects (pause, seek)

### CalendarService (2-3 days)
- Calendar event access
- Event creation
- Reminders

### SensorService (2-3 days)
- Accelerometer, gyroscope, magnetometer
- Orientation detection
- Vibration control

---

## Phase 4: Testing & Documentation (2 weeks)

### Task 48: Create comprehensive tests

```csharp
// Sample test structure
[TestFixture]
public class PlatformServiceIntegrationTests
{
    [Test]
    [Category("iOS")]
    public async Task LocationService_iOS_GetLocation_ReturnsValidCoordinates() { }

    [Test]
    [Category("Android")]
    public async Task LocationService_Android_GetLocation_ReturnsValidCoordinates() { }

    [Test]
    [Category("Windows")]
    public async Task LocationService_Windows_GetLocation_ReturnsValidCoordinates() { }

    [Test]
    [Category("Integration")]
    public async Task MultipleServices_Work_Together() { }
}
```

- 5-8 tests per service (platform-specific + cross-platform)
- Total: 50+ unit tests
- 10+ integration tests
- 100% code coverage for shared logic

### Task 49: Documentation

**Files to create:**
- `README.md` - Overview of all services
- `docs/PLATFORM-SETUP.md` - iOS, Android, Windows, macOS setup
- `docs/API-REFERENCE.md` - All services documented
- `docs/PERMISSIONS.md` - Permission requirements per platform
- `docs/USAGE-EXAMPLES.md` - Code examples for each service

---

## Summary

### Deliverables
- ✅ 10 platform services fully implemented
- ✅ 4 platform implementations each (iOS, Android, Windows, macOS)
- ✅ 50+ unit tests with 80%+ coverage
- ✅ 10+ integration tests
- ✅ 100% XML documentation
- ✅ Comprehensive guides and examples

### Services Implemented
1. ✅ LocationService (GPS/Geolocation)
2. ✅ CameraService (Photo/Video)
3. ✅ BiometricService (Fingerprint/Face)
4. ✅ FilePickerService (File selection)
5. ✅ PermissionService (Unified permissions)
6. ✅ NotificationService (Local notifications)
7. ✅ AudioService (Audio playback/recording)
8. ✅ CalendarService (Calendar integration)
9. ✅ SensorService (Accelerometer, gyro, compass)
10. ✅ ContactsService (Already implemented)

### Success Metrics
- [ ] All 10 services implemented
- [ ] 4 platform implementations per service
- [ ] 80%+ test coverage
- [ ] All services documented
- [ ] Platform-specific guides complete
- [ ] Example apps for each service

### Timeline
- **Phase 1** (2-3 weeks): Location service
- **Phase 2** (2 weeks): Camera service
- **Phase 3** (3-4 weeks): 7 additional services
- **Phase 4** (2 weeks): Tests & documentation

**Total: 9-11 weeks, 3-4 developers (mobile specialists)**

---

**Next:** Move to SmartWorkz.Core.Shared implementation plan
