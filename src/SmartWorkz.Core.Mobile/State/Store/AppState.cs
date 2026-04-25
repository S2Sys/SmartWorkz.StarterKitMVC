namespace SmartWorkz.Mobile.State.Store;

/// <summary>Root application state (immutable).</summary>
public class AppState
{
    /// <summary>Current app initialization state.</summary>
    public InitializationState InitState { get; init; } = new();

    /// <summary>User authentication state.</summary>
    public AuthState AuthState { get; init; } = new();

    /// <summary>Sync/offline state.</summary>
    public SyncState SyncState { get; init; } = new();

    /// <summary>Notification state.</summary>
    public NotificationState NotificationState { get; init; } = new();

    /// <summary>Error state.</summary>
    public ErrorState ErrorState { get; init; } = new();

    /// <summary>Test value for unit testing.</summary>
    public int TestValue { get; init; }

    /// <summary>Create new state with updated values (for immutability).</summary>
    public AppState With(
        InitializationState? initState = null,
        AuthState? authState = null,
        SyncState? syncState = null,
        NotificationState? notificationState = null,
        ErrorState? errorState = null,
        int? testValue = null)
    {
        return new AppState
        {
            InitState = initState ?? InitState,
            AuthState = authState ?? AuthState,
            SyncState = syncState ?? SyncState,
            NotificationState = notificationState ?? NotificationState,
            ErrorState = errorState ?? ErrorState,
            TestValue = testValue ?? TestValue
        };
    }
}

/// <summary>Application initialization state.</summary>
public class InitializationState
{
    /// <summary>Whether app has completed initialization.</summary>
    public bool IsInitialized { get; init; }

    /// <summary>Whether initialization is in progress.</summary>
    public bool IsInitializing { get; init; }

    /// <summary>Initialization error message, if any.</summary>
    public string? InitError { get; init; }
}

/// <summary>User authentication and authorization state.</summary>
public class AuthState
{
    /// <summary>Whether user is authenticated.</summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>Current user ID.</summary>
    public string? UserId { get; init; }

    /// <summary>Current access token.</summary>
    public string? AccessToken { get; init; }

    /// <summary>When the access token expires.</summary>
    public DateTime? TokenExpiresAt { get; init; }
}

/// <summary>Synchronization and offline state.</summary>
public class SyncState
{
    /// <summary>Whether sync is currently in progress.</summary>
    public bool IsSyncing { get; init; }

    /// <summary>Last successful sync timestamp.</summary>
    public DateTime? LastSyncTime { get; init; }

    /// <summary>Number of pending changes awaiting sync.</summary>
    public int PendingChanges { get; init; }

    /// <summary>List of sync errors encountered.</summary>
    public List<string> SyncErrors { get; init; } = new();
}

/// <summary>Push notification state.</summary>
public class NotificationState
{
    /// <summary>List of received push notifications.</summary>
    public List<PushNotification> Notifications { get; init; } = new();

    /// <summary>Count of unread notifications.</summary>
    public int UnreadCount { get; init; }
}

/// <summary>Individual push notification model.</summary>
public class PushNotification
{
    /// <summary>Unique notification ID.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Notification title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Notification body text.</summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>When the notification was received.</summary>
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Whether the notification has been read.</summary>
    public bool IsRead { get; init; }

    /// <summary>Additional notification data.</summary>
    public Dictionary<string, string>? Data { get; init; }
}

/// <summary>Application error state.</summary>
public class ErrorState
{
    /// <summary>Last error message.</summary>
    public string? LastError { get; init; }

    /// <summary>Last error code or identifier.</summary>
    public string? LastErrorCode { get; init; }

    /// <summary>When the last error occurred.</summary>
    public DateTime? ErrorOccurredAt { get; init; }
}
