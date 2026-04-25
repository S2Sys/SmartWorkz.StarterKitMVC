using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Actions;

/// <summary>Initializes the application and begins setup sequence.</summary>
public class InitializeAppAction : IAction
{
    public string Type => nameof(InitializeAppAction);
}

/// <summary>Stores authentication token and user information.</summary>
public class SetAuthTokenAction : IAction
{
    /// <summary>Authenticated user ID.</summary>
    public required string UserId { get; init; }

    /// <summary>Bearer token for API requests.</summary>
    public required string Token { get; init; }

    /// <summary>When the token expires.</summary>
    public required DateTime ExpiresAt { get; init; }

    public string Type => nameof(SetAuthTokenAction);
}

/// <summary>Clears all authentication state (logout).</summary>
public class ClearAuthStateAction : IAction
{
    public string Type => nameof(ClearAuthStateAction);
}

/// <summary>Updates the current synchronization state.</summary>
public class SetSyncStateAction : IAction
{
    /// <summary>Whether sync is currently in progress.</summary>
    public required bool IsSyncing { get; init; }

    /// <summary>Optional last sync timestamp.</summary>
    public DateTime? LastSyncTime { get; init; }

    /// <summary>Optional count of pending changes.</summary>
    public int? PendingChanges { get; init; }

    public string Type => nameof(SetSyncStateAction);
}

/// <summary>Adds a new push notification to the notification state.</summary>
public class AddNotificationAction : IAction
{
    /// <summary>The notification to add.</summary>
    public required PushNotification Notification { get; init; }

    public string Type => nameof(AddNotificationAction);
}

/// <summary>Marks a notification as read.</summary>
public class MarkNotificationAsReadAction : IAction
{
    /// <summary>ID of the notification to mark as read.</summary>
    public required string NotificationId { get; init; }

    public string Type => nameof(MarkNotificationAsReadAction);
}

/// <summary>Records an application error.</summary>
public class SetErrorAction : IAction
{
    /// <summary>Error message.</summary>
    public required string Error { get; init; }

    /// <summary>Optional error code for error categorization.</summary>
    public string? Code { get; init; }

    public string Type => nameof(SetErrorAction);
}

/// <summary>Clears the current error state.</summary>
public class ClearErrorAction : IAction
{
    public string Type => nameof(ClearErrorAction);
}
