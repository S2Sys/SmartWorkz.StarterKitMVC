namespace SmartWorkz.Mobile.Notifications;

/// <summary>
/// Manages push notification permission checking and requesting across platform handlers.
/// Provides caching to avoid redundant permission checks and handles permission state changes.
/// </summary>
public class PushNotificationPermissionHelper
{
    private readonly Dictionary<IPushNotificationHandler, bool?> _permissionCache = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Event raised when the permission status changes for any handler.
    /// </summary>
    public event EventHandler<string>? PermissionStatusChanged;

    /// <summary>
    /// Checks if the handler has push notification permissions without prompting the user.
    /// Results are cached to avoid repeated permission checks.
    /// </summary>
    /// <param name="handler">The push notification handler to check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if permissions are granted; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
    public async Task<bool> HasPermissionAsync(IPushNotificationHandler handler, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lockObject)
        {
            if (_permissionCache.TryGetValue(handler, out var cached) && cached.HasValue)
            {
                return cached.Value;
            }
        }

        var result = await handler.HasPermissionAsync(cancellationToken);

        lock (_lockObject)
        {
            _permissionCache[handler] = result;
        }

        return result;
    }

    /// <summary>
    /// Requests push notification permissions from the user.
    /// The request is platform-specific: iOS shows a system dialog, Android checks runtime permissions.
    /// Results are cached and cached status is cleared on denial.
    /// </summary>
    /// <param name="handler">The push notification handler to request permissions from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if permissions were granted; false if denied or cancelled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
    public async Task<bool> RequestPermissionsAsync(IPushNotificationHandler handler, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var result = await handler.RequestPermissionsAsync(cancellationToken);

        lock (_lockObject)
        {
            if (result)
            {
                _permissionCache[handler] = true;
                PermissionStatusChanged?.Invoke(this, $"{handler.PlatformName}: Permission granted");
            }
            else
            {
                _permissionCache.Remove(handler);
                PermissionStatusChanged?.Invoke(this, $"{handler.PlatformName}: Permission denied");
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the cached permission status for a handler without making any checks.
    /// </summary>
    /// <param name="handler">The push notification handler to check the cache for.</param>
    /// <returns>The cached permission status, or null if not cached.</returns>
    public bool? GetCachedStatus(IPushNotificationHandler handler)
    {
        lock (_lockObject)
        {
            _permissionCache.TryGetValue(handler, out var cached);
            return cached;
        }
    }

    /// <summary>
    /// Clears all cached permission statuses.
    /// This forces fresh permission checks on the next HasPermissionAsync call.
    /// </summary>
    public void ClearCache()
    {
        lock (_lockObject)
        {
            _permissionCache.Clear();
        }
    }

    /// <summary>
    /// Clears the cached permission status for a specific handler.
    /// </summary>
    /// <param name="handler">The handler whose cache should be cleared.</param>
    public void ClearCacheForHandler(IPushNotificationHandler handler)
    {
        lock (_lockObject)
        {
            _permissionCache.Remove(handler);
        }
    }

    /// <summary>
    /// Gets the number of handlers with cached permission statuses.
    /// </summary>
    public int CachedHandlerCount
    {
        get
        {
            lock (_lockObject)
            {
                return _permissionCache.Count;
            }
        }
    }
}
