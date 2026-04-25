namespace SmartWorkz.Mobile.State.Store;

/// <summary>
/// Root application state record holding all sub-states.
/// Immutable record with init-only properties for Redux-style pure state updates.
/// </summary>
public record AppState(
    InitState InitState = default,
    AuthState AuthState = default,
    SyncState SyncState = default,
    NotificationState NotificationState = default,
    ErrorState ErrorState = default,
    int TestValue = 0)
{
    /// <summary>Initialization state - tracks app startup and setup.</summary>
    public InitState InitState { get; init; } = InitState ?? new();

    /// <summary>Authentication state - tracks user login and session.</summary>
    public AuthState AuthState { get; init; } = AuthState ?? new();

    /// <summary>Synchronization state - tracks sync progress and pending changes.</summary>
    public SyncState SyncState { get; init; } = SyncState ?? new();

    /// <summary>Notification state - tracks push notifications and unread count.</summary>
    public NotificationState NotificationState { get; init; } = NotificationState ?? new();

    /// <summary>Error state - tracks application errors.</summary>
    public ErrorState ErrorState { get; init; } = ErrorState ?? new();

    /// <summary>Test value for testing immutability behavior.</summary>
    public int TestValue { get; init; } = TestValue;

    /// <summary>
    /// Create a new AppState with selective property updates.
    /// </summary>
    public AppState With(
        InitState? initState = null,
        AuthState? authState = null,
        SyncState? syncState = null,
        NotificationState? notificationState = null,
        ErrorState? errorState = null,
        int? testValue = null)
    {
        return new AppState(
            initState ?? this.InitState,
            authState ?? this.AuthState,
            syncState ?? this.SyncState,
            notificationState ?? this.NotificationState,
            errorState ?? this.ErrorState,
            testValue ?? this.TestValue);
    }
}

/// <summary>
/// Application initialization state.
/// Tracks the startup sequence and any initialization errors.
/// </summary>
public record InitState
{
    /// <summary>Whether the app is currently initializing.</summary>
    public bool IsInitializing { get; init; }

    /// <summary>Whether the app has completed initialization.</summary>
    public bool IsInitialized { get; init; }

    /// <summary>Error message if initialization failed.</summary>
    public string? InitError { get; init; }
}

/// <summary>
/// Authentication state.
/// Tracks user identity, authentication tokens, and session information.
/// </summary>
public record AuthState
{
    /// <summary>Whether the user is currently authenticated.</summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>Authenticated user's unique identifier.</summary>
    public string? UserId { get; init; }

    /// <summary>Bearer token for authenticated API requests.</summary>
    public string? AccessToken { get; init; }

    /// <summary>When the access token expires.</summary>
    public DateTime? TokenExpiresAt { get; init; }
}

/// <summary>
/// Synchronization state.
/// Tracks the status of data synchronization with the server.
/// </summary>
public record SyncState
{
    /// <summary>Whether a sync operation is currently in progress.</summary>
    public bool IsSyncing { get; init; }

    /// <summary>Timestamp of the last successful sync (null if never synced).</summary>
    public DateTime? LastSyncTime { get; init; }

    /// <summary>Count of changes pending synchronization.</summary>
    public int PendingChanges { get; init; }
}

/// <summary>
/// Notification state.
/// Tracks all received push notifications and unread count.
/// </summary>
public record NotificationState
{
    /// <summary>List of all received notifications.</summary>
    public IReadOnlyList<PushNotification> Notifications { get; init; } = new List<PushNotification>();

    /// <summary>Count of unread notifications.</summary>
    public int UnreadCount { get; init; }
}

/// <summary>
/// A single push notification.
/// </summary>
public record PushNotification
{
    /// <summary>Unique identifier for the notification.</summary>
    public required string Id { get; init; }

    /// <summary>Notification title.</summary>
    public required string Title { get; init; }

    /// <summary>Notification body text.</summary>
    public required string Body { get; init; }

    /// <summary>Whether the notification has been read by the user.</summary>
    public bool IsRead { get; init; }
}

/// <summary>
/// Error state.
/// Tracks the most recent application error if one occurred.
/// </summary>
public record ErrorState
{
    /// <summary>Last error message.</summary>
    public string? LastError { get; init; }

    /// <summary>Last error code for error categorization.</summary>
    public string? LastErrorCode { get; init; }

    /// <summary>When the last error occurred.</summary>
    public DateTime? ErrorOccurredAt { get; init; }
}
