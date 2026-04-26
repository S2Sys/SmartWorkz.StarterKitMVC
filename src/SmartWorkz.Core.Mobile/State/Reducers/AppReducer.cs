using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>
/// Root reducer handling all app-level state changes.
/// Implements pure function state transitions for initialization, authentication,
/// synchronization, notifications, and error handling.
/// </summary>
public class AppReducer : IReducer
{
    /// <summary>Gets the set of action types this reducer handles.</summary>
    public IEnumerable<Type> HandledActionTypes => new[]
    {
        typeof(InitializeAppAction),
        typeof(SetAuthTokenAction),
        typeof(ClearAuthStateAction),
        typeof(SetSyncStateAction),
        typeof(AddNotificationAction),
        typeof(MarkNotificationAsReadAction),
        typeof(SetErrorAction),
        typeof(ClearErrorAction)
    };

    /// <summary>Apply an action to produce a new state (pure function).</summary>
    public AppState Reduce(AppState state, IAction action)
    {
        return action switch
        {
            InitializeAppAction => ReduceInitialize(state),
            SetAuthTokenAction authAction => ReduceSetAuthToken(state, authAction),
            ClearAuthStateAction => ReduceClearAuth(state),
            SetSyncStateAction syncAction => ReduceSetSyncState(state, syncAction),
            AddNotificationAction notifAction => ReduceAddNotification(state, notifAction),
            MarkNotificationAsReadAction readAction => ReduceMarkAsRead(state, readAction),
            SetErrorAction errorAction => ReduceSetError(state, errorAction),
            ClearErrorAction => ReduceClearError(state),
            _ => state
        };
    }

    /// <summary>Initialize app - set isInitializing flag and clear any previous errors.</summary>
    private static AppState ReduceInitialize(AppState state)
    {
        return state.With(
            initState: state.InitState with
            {
                IsInitializing = true,
                InitError = null
            }
        );
    }

    /// <summary>Update authentication state with token and user ID.</summary>
    private static AppState ReduceSetAuthToken(AppState state, SetAuthTokenAction action)
    {
        return state.With(
            authState: state.AuthState with
            {
                IsAuthenticated = true,
                UserId = action.UserId,
                AccessToken = action.Token,
                TokenExpiresAt = action.ExpiresAt
            }
        );
    }

    /// <summary>Clear all authentication state (logout).</summary>
    private static AppState ReduceClearAuth(AppState state)
    {
        return state.With(
            authState: new AuthState()
        );
    }

    /// <summary>Update sync state - progress, last sync time, pending changes.</summary>
    private static AppState ReduceSetSyncState(AppState state, SetSyncStateAction action)
    {
        return state.With(
            syncState: state.SyncState with
            {
                IsSyncing = action.IsSyncing,
                LastSyncTime = action.LastSyncTime ?? state.SyncState.LastSyncTime,
                PendingChanges = action.PendingChanges ?? state.SyncState.PendingChanges
            }
        );
    }

    /// <summary>Add a new notification to the list and increment unread count.</summary>
    private static AppState ReduceAddNotification(AppState state, AddNotificationAction action)
    {
        var notifications = new List<PushNotification>(state.NotificationState.Notifications)
        {
            action.Notification
        };

        return state.With(
            notificationState: state.NotificationState with
            {
                Notifications = notifications,
                UnreadCount = state.NotificationState.UnreadCount + 1
            }
        );
    }

    /// <summary>Mark a notification as read and update unread count.</summary>
    private static AppState ReduceMarkAsRead(AppState state, MarkNotificationAsReadAction action)
    {
        var updated = state.NotificationState.Notifications
            .Select(n => n.Id == action.NotificationId ? n with { IsRead = true } : n)
            .ToList();

        var unreadCount = updated.Count(n => !n.IsRead);

        return state.With(
            notificationState: state.NotificationState with
            {
                Notifications = updated,
                UnreadCount = unreadCount
            }
        );
    }

    /// <summary>Record an error in the state with timestamp.</summary>
    private static AppState ReduceSetError(AppState state, SetErrorAction action)
    {
        return state.With(
            errorState: new ErrorState
            {
                LastError = action.Error,
                LastErrorCode = action.Code,
                ErrorOccurredAt = DateTime.UtcNow
            }
        );
    }

    /// <summary>Clear the current error state.</summary>
    private static AppState ReduceClearError(AppState state)
    {
        return state.With(
            errorState: new ErrorState()
        );
    }
}
