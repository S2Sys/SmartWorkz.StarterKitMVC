using Xunit;
using SmartWorkz.Mobile.State;
using SmartWorkz.Mobile.State.Store;
using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Reducers;

namespace SmartWorkz.Mobile.Tests.State;

public class ReducerTests
{
    [Fact]
    public void AppReducer_InitializeAppAction_SetsInitializingState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState();
        var action = new InitializeAppAction();

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.True(newState.InitState.IsInitializing);
        Assert.False(newState.InitState.IsInitialized);
        Assert.Null(newState.InitState.InitError);
    }

    [Fact]
    public void AppReducer_SetAuthTokenAction_UpdatesAuthState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState();
        var action = new SetAuthTokenAction
        {
            UserId = "user123",
            Token = "token_abc",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.True(newState.AuthState.IsAuthenticated);
        Assert.Equal("user123", newState.AuthState.UserId);
        Assert.Equal("token_abc", newState.AuthState.AccessToken);
        Assert.Equal(action.ExpiresAt, newState.AuthState.TokenExpiresAt);
    }

    [Fact]
    public void AppReducer_ClearAuthStateAction_ClearsAuthentication()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState
        {
            AuthState = new AuthState
            {
                IsAuthenticated = true,
                UserId = "user123",
                AccessToken = "token_abc"
            }
        };
        var action = new ClearAuthStateAction();

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.False(newState.AuthState.IsAuthenticated);
        Assert.Null(newState.AuthState.UserId);
        Assert.Null(newState.AuthState.AccessToken);
        Assert.Null(newState.AuthState.TokenExpiresAt);
    }

    [Fact]
    public void AppReducer_SetSyncStateAction_UpdatesSyncState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState();
        var lastSyncTime = DateTime.UtcNow;
        var action = new SetSyncStateAction
        {
            IsSyncing = true,
            LastSyncTime = lastSyncTime,
            PendingChanges = 5
        };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.True(newState.SyncState.IsSyncing);
        Assert.Equal(lastSyncTime, newState.SyncState.LastSyncTime);
        Assert.Equal(5, newState.SyncState.PendingChanges);
    }

    [Fact]
    public void AppReducer_AddNotificationAction_AddsNotificationToState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState();
        var notification = new PushNotification
        {
            Id = "notif123",
            Title = "Test Notification",
            Body = "This is a test"
        };
        var action = new AddNotificationAction { Notification = notification };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Single(newState.NotificationState.Notifications);
        Assert.Equal("notif123", newState.NotificationState.Notifications[0].Id);
        Assert.Equal("Test Notification", newState.NotificationState.Notifications[0].Title);
        Assert.Equal(1, newState.NotificationState.UnreadCount);
    }

    [Fact]
    public void AppReducer_MarkNotificationAsReadAction_UpdatesReadStatus()
    {
        // Arrange
        var reducer = new AppReducer();
        var notification = new PushNotification
        {
            Id = "notif123",
            Title = "Test",
            Body = "Test",
            IsRead = false
        };
        var state = new AppState
        {
            NotificationState = new NotificationState
            {
                Notifications = new[] { notification },
                UnreadCount = 1
            }
        };
        var action = new MarkNotificationAsReadAction { NotificationId = "notif123" };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Single(newState.NotificationState.Notifications);
        Assert.True(newState.NotificationState.Notifications[0].IsRead);
        Assert.Equal(0, newState.NotificationState.UnreadCount);
    }

    [Fact]
    public void AppReducer_SetErrorAction_UpdatesErrorState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState();
        var action = new SetErrorAction
        {
            Error = "Connection failed",
            Code = "CONN_ERR"
        };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Equal("Connection failed", newState.ErrorState.LastError);
        Assert.Equal("CONN_ERR", newState.ErrorState.LastErrorCode);
        Assert.NotNull(newState.ErrorState.ErrorOccurredAt);
    }

    [Fact]
    public void AppReducer_ClearErrorAction_ClearsErrorState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState
        {
            ErrorState = new ErrorState
            {
                LastError = "Previous error",
                LastErrorCode = "ERR_001",
                ErrorOccurredAt = DateTime.UtcNow
            }
        };
        var action = new ClearErrorAction();

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Null(newState.ErrorState.LastError);
        Assert.Null(newState.ErrorState.LastErrorCode);
        Assert.Null(newState.ErrorState.ErrorOccurredAt);
    }

    [Fact]
    public void AppReducer_UnknownAction_ReturnsUnchangedState()
    {
        // Arrange
        var reducer = new AppReducer();
        var state = new AppState { TestValue = 999 };
        var action = new UnknownTestAction();

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Equal(state, newState);
    }

    [Fact]
    public void AppReducer_ImmutabilityPreserved_OriginalStateUnchanged()
    {
        // Arrange
        var reducer = new AppReducer();
        var originalState = new AppState();
        var action = new InitializeAppAction();

        // Act
        var newState = reducer.Reduce(originalState, action);

        // Assert
        Assert.NotSame(originalState, newState);
        Assert.False(originalState.InitState.IsInitializing);
        Assert.True(newState.InitState.IsInitializing);
    }

    [Fact]
    public void AppReducer_MultipleNotifications_PreservesExistingOnes()
    {
        // Arrange
        var reducer = new AppReducer();
        var notif1 = new PushNotification { Id = "notif1", Title = "First" };
        var notif2 = new PushNotification { Id = "notif2", Title = "Second" };
        var state = new AppState
        {
            NotificationState = new NotificationState
            {
                Notifications = new[] { notif1, notif2 },
                UnreadCount = 2
            }
        };
        var newNotif = new PushNotification { Id = "notif3", Title = "Third" };
        var action = new AddNotificationAction { Notification = newNotif };

        // Act
        var newState = reducer.Reduce(state, action);

        // Assert
        Assert.Equal(3, newState.NotificationState.Notifications.Count);
        Assert.Equal("notif1", newState.NotificationState.Notifications[0].Id);
        Assert.Equal("notif2", newState.NotificationState.Notifications[1].Id);
        Assert.Equal("notif3", newState.NotificationState.Notifications[2].Id);
        Assert.Equal(3, newState.NotificationState.UnreadCount);
    }
}

/// <summary>Test action for unknown action handling.</summary>
public class UnknownTestAction : IAction
{
    public string Type => nameof(UnknownTestAction);
}
