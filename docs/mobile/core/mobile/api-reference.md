# Mobile API Reference

## Classes & Interfaces

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Mobile.ServiceCollectionExtensions`
- **Summary:** Extension methods for configuring SmartWorkz.Core.MAUI services in the dependency injection container.

#### Methods & Properties

- **AddSmartWorkzCoreMobile** - Adds SmartWorkz.Core.MAUI services to the dependency injection container with Phase B configuration.
            
             Registers platform services, connectivity checks, storage, authentication, and synchronization capabilities.
             Supports configurable interceptors, analytics, and rate limiting.
  - Parameters:
    - `services`: The service collection to add services to.
    - `configureApi`: Optional action to configure mobile API settings.
    - `enableBuiltinInterceptors`: Whether to register built-in interceptors (CorrelationInterceptor and DeviceInfoInterceptor). Default: true.
    - `enableRealAnalytics`: Whether to register BackendAnalyticsService. If false, registers stub AnalyticsService. Default: false.
  - Returns: The service collection for method chaining.

### Contact

- **Namespace:** `SmartWorkz.Mobile.Contact`
- **Summary:** Represents a contact from the device address book.

#### Methods & Properties

- **#ctor** - Represents a contact from the device address book.

### GpsLocation

- **Namespace:** `SmartWorkz.Mobile.GpsLocation`
- **Summary:** Represents a GPS location with coordinates and optional metadata.

#### Methods & Properties

- **#ctor** - Represents a GPS location with coordinates and optional metadata.
- **DistanceTo** - Calculates the distance in kilometers between this location and another using the Haversine formula.
  - Parameters:
    - `other`: The other location to calculate distance to
  - Returns: Distance in kilometers
- **DistanceToMeters** - Calculates the distance in meters between this location and another using the Haversine formula.
  - Parameters:
    - `other`: The other location to calculate distance to
  - Returns: Distance in meters
- **IsApproximatelyEqual** - Checks if this location is approximately equal to another (within specified accuracy in kilometers).
  - Parameters:
    - `other`: The other location to compare with
    - `toleranceKm`: Tolerance in kilometers (default: 0.01 km = 10 meters)
  - Returns: True if locations are within tolerance distance

### BiometricService

- **Namespace:** `SmartWorkz.Mobile.BiometricService`
- **Summary:** Provides biometric authentication services (fingerprint, face recognition, etc.).

#### Methods & Properties

- **IsAvailableAsync** - Checks if biometric authentication is available on the device.
- **GetBiometricTypeAsync** - Gets the type of biometric authentication available on the device.
- **AuthenticateAsync** - Authenticates the user using biometric authentication.

### CameraService

- **Namespace:** `SmartWorkz.Mobile.CameraService`
- **Summary:** Provides camera services for capturing photos and videos.

#### Methods & Properties

- **IsCameraAvailableAsync** - Checks if camera hardware is available on the device.
- **TakePhotoAsync** - Launches the camera to capture a photo.
            Permission must be granted before calling.
- **RecordVideoAsync** - Launches the camera to record a video.
            Permission must be granted before calling.
- **GetCameraFolder** - Gets the path where camera files are stored on this platform.

### ContactsService

- **Namespace:** `SmartWorkz.Mobile.ContactsService`
- **Summary:** Provides contact access services for retrieving and searching device contacts.

#### Methods & Properties

- **GetAllContactsAsync** - Retrieves all contacts from the device.
- **SearchContactsAsync** - Searches for contacts matching the given query.
- **PickContactAsync** - Picks a single contact using the device's native contact picker.
- **IsAvailableAsync** - Checks if contact access is available on the device.

### LocationService

- **Namespace:** `SmartWorkz.Mobile.LocationService`
- **Summary:** Provides location services for accessing device geolocation and location tracking.

#### Methods & Properties

- **GetCurrentLocationAsync** - Gets the current device location.
            Permission must be granted before calling.
- **StartTracking** - Starts continuous location tracking.
            Returns an observable stream of location updates.
- **StopTracking** - Stops continuous location tracking.
- **IsAvailableAsync** - Checks if location services are available and enabled on the device.
- **OnLocationChanged** - Raises the LocationChanged event when a location update occurs.

### MediaPickerService

- **Namespace:** `SmartWorkz.Mobile.MediaPickerService`
- **Summary:** Provides media picker services for selecting photos and videos from device storage.

#### Methods & Properties

- **PickPhotoAsync** - Launches the media picker to select a photo.
            Permission must be granted before calling.
- **PickVideoAsync** - Launches the media picker to select a video.
            Permission must be granted before calling.
- **PickMultipleAsync** - Launches the media picker to select multiple media files.
            Permission must be granted before calling.
- **IsAvailableAsync** - Checks if media picker is available on the device.

### PushNotificationClientService

- **Namespace:** `SmartWorkz.Mobile.PushNotificationClientService`
- **Summary:** Cross-platform push notification registration service.

### ICameraService

- **Namespace:** `SmartWorkz.Mobile.ICameraService`
- **Summary:** Cross-platform camera service for capturing photos and videos.
            Handles permission checks and platform-specific camera access.

#### Methods & Properties

- **IsCameraAvailableAsync** - Checks if camera hardware is available on the device.
- **TakePhotoAsync** - Launches the camera to capture a photo.
            Permission must be granted before calling.
  - Returns: FileResult with path to photo, or null if cancelled
- **RecordVideoAsync** - Launches the camera to record a video.
            Permission must be granted before calling.
  - Returns: FileResult with path to video, or null if cancelled
- **GetCameraFolder** - Gets the path where camera files are stored on this platform.

### IContactsService

- **Namespace:** `SmartWorkz.Mobile.IContactsService`
- **Summary:** Service for accessing device contacts from the address book.

#### Methods & Properties

- **GetAllContactsAsync** - Gets all contacts from the device address book.
            Requires READ_CONTACTS (Android) or Contacts permission (iOS).
- **SearchContactsAsync** - Searches contacts by name or email.
            Requires READ_CONTACTS (Android) or Contacts permission (iOS).
- **PickContactAsync** - Picks a single contact from the device.
            Requires READ_CONTACTS (Android) or Contacts permission (iOS).
            On unsupported platforms, returns null.
- **IsAvailableAsync** - Checks if contacts service is available on this platform.

### ILocationService

- **Namespace:** `SmartWorkz.Mobile.ILocationService`
- **Summary:** Service for accessing device location via GPS.

#### Methods & Properties

- **GetCurrentLocationAsync** - Gets the current device location.
            Requires LOCATION or FINE_LOCATION permission (Android) or Location permission (iOS).
- **StartTracking** - Starts continuous location tracking.
            Requires LOCATION or FINE_LOCATION permission (Android) or Location permission (iOS).
            Returns an observable stream of location updates.
- **StopTracking** - Stops continuous location tracking.
- **IsAvailableAsync** - Checks if location service is available on this platform.

### LocationTrackingOptions

- **Namespace:** `SmartWorkz.Mobile.LocationTrackingOptions`
- **Summary:** Options for configuring location tracking behavior.

#### Methods & Properties

- **#ctor** - Options for configuring location tracking behavior.

### LocationAccuracy

- **Namespace:** `SmartWorkz.Mobile.LocationAccuracy`
- **Summary:** Location accuracy levels for platform-specific configuration.

### IMediaPickerService

- **Namespace:** `SmartWorkz.Mobile.IMediaPickerService`
- **Summary:** Cross-platform media picker service for selecting photos and videos from device storage.
            Handles permission checks and platform-specific media picker access.

#### Methods & Properties

- **PickPhotoAsync** - Launches the media picker to select a photo.
            Permission must be granted before calling.
  - Returns: FileResult with path to selected photo, or null if cancelled
- **PickVideoAsync** - Launches the media picker to select a video.
            Permission must be granted before calling.
  - Returns: FileResult with path to selected video, or null if cancelled
- **PickMultipleAsync** - Launches the media picker to select multiple media files.
            Permission must be granted before calling.
  - Returns: Collection of FileResult with paths to selected media, or empty if cancelled
- **IsAvailableAsync** - Checks if media picker is available on the device.

### IMobileContext

- **Namespace:** `SmartWorkz.Mobile.IMobileContext`
- **Summary:** Provides stable platform and device context information.
            This is a lightweight data holder with no service dependencies,
            designed to break circular dependency patterns between ErrorHandler and MobileService.

### BackendAnalyticsService

- **Namespace:** `SmartWorkz.Mobile.BackendAnalyticsService`
- **Summary:** Sends analytics events to the backend API with rate limiting.

### MobileContext

- **Namespace:** `SmartWorkz.Mobile.MobileContext`
- **Summary:** Provides mutable platform and device context information.
            This is a simple data holder designed to break circular dependency patterns.

### RequestDeduplicationService

- **Namespace:** `SmartWorkz.Mobile.RequestDeduplicationService`
- **Summary:** Prevents duplicate API requests in-flight by caching and reusing Task results.
            Thread-safe dictionary maintains in-flight requests keyed by (method:endpoint).
            When same request is detected, returns cached Task instead of executing.
            After execution completes, entry is cleaned up.

### ResultExtensions

- **Namespace:** `SmartWorkz.Mobile.ResultExtensions`
- **Summary:** Extension method to convert Result{T} to non-generic Result.

### IResponseInterceptor

- **Namespace:** `SmartWorkz.Mobile.IResponseInterceptor`
- **Summary:** Extends IRequestInterceptor to provide response handling capabilities.
            Implementations can process HTTP responses for logging, retry logic, token refresh, and other cross-cutting concerns.

#### Methods & Properties

- **OnResponseAsync** - Called after a response is received. Processes response for logging, retry logic, token refresh, etc.
  - Parameters:
    - `response`: The HTTP response message.
    - `ct`: Cancellation token.
  - Returns: True if the interceptor handled the response and the request should be retried; false otherwise.

### ITokenRefreshInterceptor

- **Namespace:** `SmartWorkz.Mobile.ITokenRefreshInterceptor`
- **Summary:** Extends IResponseInterceptor to provide token refresh capability.
            Automatically refreshes JWT tokens when 401 Unauthorized responses are encountered.

### RequestLoggingInterceptor

- **Namespace:** `SmartWorkz.Mobile.RequestLoggingInterceptor`
- **Summary:** Interceptor that logs HTTP request and response details including method, URI, status code, and elapsed time.
            Optional body logging is disabled by default and should only be enabled in Development environment.

#### Methods & Properties

- **InterceptAsync** - Called before the request is sent. Logs request method and URI, starts timing.
- **OnResponseAsync** - Called after a response is received. Logs status code, elapsed time, and optional body.
  - Parameters:
    - `response`: The HTTP response message.
    - `ct`: Cancellation token.
  - Returns: False (never retries, this is just logging).

### TokenRefreshInterceptor

- **Namespace:** `SmartWorkz.Mobile.TokenRefreshInterceptor`
- **Summary:** Interceptor that automatically refreshes JWT tokens when 401 Unauthorized responses are encountered.
            Prevents multiple concurrent refresh attempts using a SemaphoreSlim for thread-safe mutual exclusion.
            Implements IResponseInterceptor via ITokenRefreshInterceptor to handle response processing.

#### Methods & Properties

- **InterceptAsync** - Called before the request is sent. Currently a no-op for token refresh.
- **OnResponseAsync** - Called after a response is received. Detects 401 status codes and attempts token refresh.
  - Parameters:
    - `response`: The HTTP response message.
    - `ct`: Cancellation token.
  - Returns: True if the response was a 401 and token refresh succeeded (request should retry); false otherwise.

### IRequestDeduplicationService

- **Namespace:** `SmartWorkz.Mobile.IRequestDeduplicationService`
- **Summary:** Service that prevents duplicate API requests in-flight by caching and reusing Task results.
            If the same request is made twice concurrently (same key), returns the same Task instead of executing twice.
            Cache entries are released after completion (both success and failure).

#### Methods & Properties

- **GetOrExecuteAsync``1** - Executes an async operation or returns a cached Task if the same key is already in-flight.
            Prevents duplicate concurrent requests. Cache is cleared after operation completes (success or failure).
  - Parameters:
    - `key`: The unique key identifying the request (e.g., "GET:/api/users/123").
    - `executeAsync`: The async factory function to execute if not already in-flight.
    - `ct`: Optional cancellation token.
  - Returns: A Task that resolves to a Result{T}. If the same key is in-flight, the same Task is returned.

### Resource

- **Namespace:** `SmartWorkz.Mobile.Resource`
- **Summary:** Android Resource Designer class.
            Exposes the Android Resource designer assembly into the project Namespace.

