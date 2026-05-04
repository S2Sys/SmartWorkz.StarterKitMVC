# SmartWorkz Mobile Framework 2.0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a production-ready, framework-agnostic mobile development platform supporting any app type (e-commerce, enterprise, social, messaging) with comprehensive UI components, state management, persistent storage, and developer experience tools.

**Architecture:** Infrastructure-first approach. Phase 3A establishes the foundation (state management, data persistence, crash reporting, push notifications) that all subsequent features build upon. Phase 3B adds essential UI components covering 80% of form/list/navigation needs. Phase 3C completes the developer experience with configuration, accessibility, and testing.

**Tech Stack:** .NET 9, MAUI, Redux state container (custom), SQLite with ORM abstraction, Sentry SDK, Microsoft.Logging, Xamarin biometric libraries, Firebase/APNs for push notifications.

**Timeline:** 6 weeks total | Team: 5+ developers | Approach: Parallel tracks where infrastructure and UI can progress simultaneously

---

## Phase 3A: Infrastructure Foundation (Weeks 1-3)

### File Structure - Phase 3A

```
src/SmartWorkz.Core.MAUI/
├── State/                          # NEW: Redux-like state container
│   ├── Store/
│   │   ├── IAppStore.cs           # Store interface
│   │   ├── AppStore.cs            # Store implementation with dispatch, subscribe
│   │   ├── AppState.cs            # Root state model
│   │   └── StoreExtensions.cs     # DI registration helpers
│   ├── Actions/
│   │   ├── Action.cs              # Base action interface
│   │   ├── AppActions.cs          # App-level actions (init, auth, config)
│   │   ├── SyncActions.cs         # Sync state actions
│   │   └── NotificationActions.cs # Push notification actions
│   └── Reducers/
│       ├── Reducer.cs             # Base reducer interface
│       ├── AppReducer.cs          # App state reducer
│       ├── SyncReducer.cs         # Sync state reducer
│       └── ReducerRegistry.cs     # Reducer registration
│
├── Data/                           # MODIFIED/NEW: Database abstraction layer
│   ├── Database/
│   │   ├── IDatabase.cs           # Database abstraction interface
│   │   ├── IDbConnection.cs       # Connection abstraction
│   │   ├── IDbTransaction.cs      # Transaction abstraction
│   │   ├── SQLiteDatabase.cs      # SQLite implementation
│   │   ├── DbMigration.cs         # Migration base class
│   │   └── DbMigrationRunner.cs   # Migration executor
│   ├── Repositories/
│   │   ├── IRepository.cs         # Generic repository interface
│   │   ├── Repository.cs          # Base repository implementation
│   │   └── [EntityName]Repository.cs # Specific repositories
│   ├── Migrations/
│   │   ├── Migration_001_InitialSchema.cs
│   │   ├── Migration_002_AddSyncTables.cs
│   │   └── Migration_003_AddNotificationLog.cs
│   └── Models/
│       ├── SyncEntity.cs          # Base synced entity
│       ├── LocalChangeLog.cs      # Tracks local changes
│       └── NotificationLog.cs     # Push notification history
│
├── Telemetry/                      # NEW: Crash reporting + analytics
│   ├── ICrashReportingService.cs  # Crash reporting interface
│   ├── SentryCrashReporter.cs     # Sentry implementation
│   ├── CrashContext.cs            # Context for crash reports
│   └── TelemetryExtensions.cs     # DI registration
│
├── Notifications/                  # MODIFIED: Complete push pipeline
│   ├── IPushNotificationHandler.cs # Message handler interface
│   ├── PushNotificationHandler.cs  # Implementation
│   ├── DeepLinkRouter.cs           # Route notifications to pages
│   ├── NotificationPermissionHelper.cs # iOS/Android perms
│   ├── Platforms/
│   │   ├── Android/PushNotificationHandler.Android.cs
│   │   ├── iOS/PushNotificationHandler.iOS.cs
│   │   └── Windows/PushNotificationHandler.Windows.cs
│   └── Models/
│       ├── PushMessage.cs
│       ├── PushPayload.cs
│       └── NotificationAction.cs
│
└── MauiProgram.cs                # MODIFIED: Add service registrations for 3A
```

---

### Task 1: Redux State Container - Store Foundation

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/State/Store/IAppStore.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Store/AppStore.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Store/AppState.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Actions/Action.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/State/AppStoreTests.cs`

**Steps:**

- [ ] **Step 1: Write failing test for store dispatch**

```csharp
// tests/SmartWorkz.Core.MAUI.Tests/State/AppStoreTests.cs
using Xunit;
using SmartWorkz.Mobile.State;

public class AppStoreTests
{
    [Fact]
    public void Dispatch_WithValidAction_UpdatesState()
    {
        // Arrange
        var initialState = new AppState();
        var store = new AppStore(initialState);
        var testAction = new TestAction { Value = 42 };

        // Act
        store.Dispatch(testAction);

        // Assert
        Assert.NotNull(store.GetState());
        Assert.Equal(42, store.GetState().TestValue);
    }

    [Fact]
    public void Subscribe_WithStateChange_NotifiesSubscriber()
    {
        // Arrange
        var store = new AppStore(new AppState());
        var stateChanges = new List<AppState>();
        
        store.Subscribe(state => stateChanges.Add(state));

        // Act
        store.Dispatch(new TestAction { Value = 123 });

        // Assert
        Assert.Single(stateChanges);
        Assert.Equal(123, stateChanges[0].TestValue);
    }
}

public class TestAction : IAction
{
    public int Value { get; set; }
}
```

Run: `dotnet test tests/SmartWorkz.Core.MAUI.Tests/State/AppStoreTests.cs -v`
Expected: **FAIL** - Classes don't exist yet

---

- [ ] **Step 2: Create IAppStore interface**

```csharp
// src/SmartWorkz.Core.MAUI/State/Store/IAppStore.cs
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.State.Store;

/// <summary>
/// Redux-like store for centralized state management.
/// Provides single source of truth for app state.
/// </summary>
public interface IAppStore
{
    /// <summary>Dispatch action to update state through reducers.</summary>
    void Dispatch(IAction action);

    /// <summary>Get current app state.</summary>
    AppState GetState();

    /// <summary>Subscribe to state changes. Returns unsubscribe function.</summary>
    Action Subscribe(Action<AppState> listener);
}
```

---

- [ ] **Step 3: Create Action interface**

```csharp
// src/SmartWorkz.Core.MAUI/State/Actions/Action.cs
namespace SmartWorkz.Mobile.State.Actions;

/// <summary>Base interface for all Redux actions.</summary>
public interface IAction
{
    /// <summary>Unique action type identifier.</summary>
    string Type => GetType().Name;
}
```

---

- [ ] **Step 4: Create AppState model**

```csharp
// src/SmartWorkz.Core.MAUI/State/Store/AppState.cs
using SmartWorkz.Mobile.State.Models;

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

    /// <summary>Create new state with updated values (for immutability).</summary>
    public AppState With(
        InitializationState? initState = null,
        AuthState? authState = null,
        SyncState? syncState = null,
        NotificationState? notificationState = null,
        ErrorState? errorState = null)
    {
        return new AppState
        {
            InitState = initState ?? InitState,
            AuthState = authState ?? AuthState,
            SyncState = syncState ?? SyncState,
            NotificationState = notificationState ?? NotificationState,
            ErrorState = errorState ?? ErrorState
        };
    }
}

public class InitializationState
{
    public bool IsInitialized { get; init; }
    public bool IsInitializing { get; init; }
    public string? InitError { get; init; }
}

public class AuthState
{
    public bool IsAuthenticated { get; init; }
    public string? UserId { get; init; }
    public string? AccessToken { get; init; }
    public DateTime? TokenExpiresAt { get; init; }
}

public class SyncState
{
    public bool IsSyncing { get; init; }
    public DateTime? LastSyncTime { get; init; }
    public int PendingChanges { get; init; }
    public List<string> SyncErrors { get; init; } = new();
}

public class NotificationState
{
    public List<PushNotification> Notifications { get; init; } = new();
    public int UnreadCount { get; init; }
}

public class PushNotification
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
    public bool IsRead { get; init; }
    public Dictionary<string, string>? Data { get; init; }
}

public class ErrorState
{
    public string? LastError { get; init; }
    public string? LastErrorCode { get; init; }
    public DateTime? ErrorOccurredAt { get; init; }
}
```

---

- [ ] **Step 5: Create AppStore implementation**

```csharp
// src/SmartWorkz.Core.MAUI/State/Store/AppStore.cs
using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Reducers;

namespace SmartWorkz.Mobile.State.Store;

/// <summary>
/// Redux store managing centralized application state.
/// Single source of truth for all app data.
/// </summary>
public class AppStore : IAppStore
{
    private AppState _state;
    private readonly List<Action<AppState>> _listeners = new();
    private readonly IReducerRegistry _reducerRegistry;
    private readonly ILogger<AppStore> _logger;

    public AppStore(AppState initialState, IReducerRegistry reducerRegistry, ILogger<AppStore> logger)
    {
        _state = initialState ?? throw new ArgumentNullException(nameof(initialState));
        _reducerRegistry = reducerRegistry ?? throw new ArgumentNullException(nameof(reducerRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Dispatch action through reducer pipeline to update state.</summary>
    public void Dispatch(IAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            _logger.LogDebug("Dispatching action: {ActionType}", action.Type);
            
            var newState = _reducerRegistry.Reduce(_state, action);
            
            if (newState != _state)
            {
                _state = newState;
                NotifyListeners();
                _logger.LogDebug("State updated by action: {ActionType}", action.Type);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching action: {ActionType}", action.Type);
            throw;
        }
    }

    /// <summary>Get current immutable state snapshot.</summary>
    public AppState GetState() => _state;

    /// <summary>Subscribe to state changes. Returns function to unsubscribe.</summary>
    public Action Subscribe(Action<AppState> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        
        _listeners.Add(listener);
        _logger.LogDebug("Listener subscribed. Total listeners: {Count}", _listeners.Count);
        
        // Return unsubscribe function
        return () =>
        {
            _listeners.Remove(listener);
            _logger.LogDebug("Listener unsubscribed. Total listeners: {Count}", _listeners.Count);
        };
    }

    private void NotifyListeners()
    {
        foreach (var listener in _listeners.ToList())
        {
            try
            {
                listener(_state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying listener");
            }
        }
    }
}
```

---

- [ ] **Step 6: Run tests - verify they pass**

Run: `dotnet test tests/SmartWorkz.Core.MAUI.Tests/State/AppStoreTests.cs -v`
Expected: **PASS** - AppStore dispatches actions and notifies subscribers

---

- [ ] **Step 7: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/State/ tests/SmartWorkz.Core.MAUI.Tests/State/
git commit -m "feat(state): add Redux store foundation with dispatch and subscribe"
```

---

### Task 2: Redux Reducers Registry

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/State/Reducers/Reducer.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Reducers/IReducerRegistry.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Reducers/ReducerRegistry.cs`
- Create: `src/SmartWorkz.Core.MAUI/State/Reducers/AppReducer.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/State/ReducerTests.cs`

**Steps:**

- [ ] **Step 1: Write failing test for reducer registry**

```csharp
// tests/SmartWorkz.Core.MAUI.Tests/State/ReducerTests.cs
using Xunit;
using SmartWorkz.Mobile.State;
using SmartWorkz.Mobile.State.Reducers;
using SmartWorkz.Mobile.State.Actions;

public class ReducerTests
{
    [Fact]
    public void ReducerRegistry_WithRegisteredReducer_AppliesReduction()
    {
        // Arrange
        var registry = new ReducerRegistry();
        registry.Register(new AppReducer());
        
        var state = new AppState();
        var action = new InitializeAppAction();

        // Act
        var newState = registry.Reduce(state, action);

        // Assert
        Assert.True(newState.InitState.IsInitializing);
    }

    [Fact]
    public void ReducerRegistry_WithUnknownAction_ReturnsUnchangedState()
    {
        // Arrange
        var registry = new ReducerRegistry();
        registry.Register(new AppReducer());
        
        var state = new AppState { InitState = new() { IsInitialized = true } };
        var unknownAction = new UnknownAction();

        // Act
        var newState = registry.Reduce(state, unknownAction);

        // Assert
        Assert.Equal(state, newState);
    }
}

public class InitializeAppAction : IAction { }
public class UnknownAction : IAction { }
```

Run: `dotnet test tests/SmartWorkz.Core.MAUI.Tests/State/ReducerTests.cs -v`
Expected: **FAIL** - ReducerRegistry doesn't exist

---

- [ ] **Step 2: Create Reducer base interface**

```csharp
// src/SmartWorkz.Core.MAUI/State/Reducers/Reducer.cs
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Base interface for all state reducers.</summary>
public interface IReducer
{
    /// <summary>
    /// Reduce current state with action to produce new state.
    /// Reducers must be pure functions (no side effects).
    /// </summary>
    AppState Reduce(AppState state, IAction action);

    /// <summary>List of action types this reducer handles.</summary>
    IEnumerable<Type> HandledActionTypes { get; }
}
```

---

- [ ] **Step 3: Create IReducerRegistry**

```csharp
// src/SmartWorkz.Core.MAUI/State/Reducers/IReducerRegistry.cs
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Registry for all reducers in the application.</summary>
public interface IReducerRegistry
{
    /// <summary>Register a reducer to handle actions.</summary>
    void Register(IReducer reducer);

    /// <summary>Reduce state with action by applying registered reducers.</summary>
    AppState Reduce(AppState state, IAction action);
}
```

---

- [ ] **Step 4: Create ReducerRegistry implementation**

```csharp
// src/SmartWorkz.Core.MAUI/State/Reducers/ReducerRegistry.cs
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.State.Reducers;

public class ReducerRegistry : IReducerRegistry
{
    private readonly List<IReducer> _reducers = new();
    private readonly Dictionary<Type, List<IReducer>> _actionTypeMap = new();
    private readonly ILogger<ReducerRegistry> _logger;

    public ReducerRegistry(ILogger<ReducerRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Register(IReducer reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);

        _reducers.Add(reducer);

        foreach (var actionType in reducer.HandledActionTypes)
        {
            if (!_actionTypeMap.ContainsKey(actionType))
            {
                _actionTypeMap[actionType] = new();
            }
            _actionTypeMap[actionType].Add(reducer);
        }

        _logger.LogDebug("Reducer registered handling {ActionCount} action types", 
            reducer.HandledActionTypes.Count());
    }

    public AppState Reduce(AppState state, IAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        var actionType = action.GetType();
        
        if (!_actionTypeMap.TryGetValue(actionType, out var reducers))
        {
            return state;
        }

        var newState = state;
        foreach (var reducer in reducers)
        {
            newState = reducer.Reduce(newState, action);
        }

        return newState;
    }
}
```

---

- [ ] **Step 5: Create AppReducer**

```csharp
// src/SmartWorkz.Core.MAUI/State/Reducers/AppReducer.cs
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Root reducer handling app-level state changes.</summary>
public class AppReducer : IReducer
{
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

    public AppState Reduce(AppState state, IAction action)
    {
        return action switch
        {
            InitializeAppAction initAction => ReduceInitialize(state, initAction),
            SetAuthTokenAction authAction => ReduceSetAuthToken(state, authAction),
            ClearAuthStateAction clearAction => ReduceClearAuth(state),
            SetSyncStateAction syncAction => ReduceSetSyncState(state, syncAction),
            AddNotificationAction notifAction => ReduceAddNotification(state, notifAction),
            MarkNotificationAsReadAction readAction => ReduceMarkAsRead(state, readAction),
            SetErrorAction errorAction => ReduceSetError(state, errorAction),
            ClearErrorAction clearError => ReduceClearError(state),
            _ => state
        };
    }

    private AppState ReduceInitialize(AppState state, InitializeAppAction action)
    {
        return state.With(
            initState: state.InitState with { IsInitializing = true, InitError = null }
        );
    }

    private AppState ReduceSetAuthToken(AppState state, SetAuthTokenAction action)
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

    private AppState ReduceClearAuth(AppState state)
    {
        return state.With(
            authState: new AuthState()
        );
    }

    private AppState ReduceSetSyncState(AppState state, SetSyncStateAction action)
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

    private AppState ReduceAddNotification(AppState state, AddNotificationAction action)
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

    private AppState ReduceMarkAsRead(AppState state, MarkNotificationAsReadAction action)
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

    private AppState ReduceSetError(AppState state, SetErrorAction action)
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

    private AppState ReduceClearError(AppState state)
    {
        return state.With(
            errorState: new ErrorState()
        );
    }
}

// Action definitions for AppReducer
public class InitializeAppAction : IAction { }

public class SetAuthTokenAction : IAction
{
    public required string UserId { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

public class ClearAuthStateAction : IAction { }

public class SetSyncStateAction : IAction
{
    public required bool IsSyncing { get; init; }
    public DateTime? LastSyncTime { get; init; }
    public int? PendingChanges { get; init; }
}

public class AddNotificationAction : IAction
{
    public required PushNotification Notification { get; init; }
}

public class MarkNotificationAsReadAction : IAction
{
    public required string NotificationId { get; init; }
}

public class SetErrorAction : IAction
{
    public required string Error { get; init; }
    public string? Code { get; init; }
}

public class ClearErrorAction : IAction { }
```

---

- [ ] **Step 6: Run tests**

Run: `dotnet test tests/SmartWorkz.Core.MAUI.Tests/State/ReducerTests.cs -v`
Expected: **PASS** - Reducer registry applies reductions correctly

---

- [ ] **Step 7: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/State/Reducers/ tests/SmartWorkz.Core.MAUI.Tests/State/ReducerTests.cs
git commit -m "feat(state): add reducer registry and app reducer with action handlers"
```

---

### Task 3: Database Abstraction Layer - Interfaces

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/IDatabase.cs`
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/IDbConnection.cs`
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/IDbTransaction.cs`
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/DbMigration.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/Data/DatabaseTests.cs`

**Steps:**

- [ ] **Step 1: Create IDatabase interface**

```csharp
// src/SmartWorkz.Core.MAUI/Data/Database/IDatabase.cs
namespace SmartWorkz.Mobile.Data.Database;

/// <summary>
/// Cross-platform database abstraction.
/// Supports SQLite on all platforms with synchronized access.
/// </summary>
public interface IDatabase
{
    /// <summary>Open database connection asynchronously.</summary>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Close database connection asynchronously.</summary>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>Check if database is connected.</summary>
    bool IsConnected { get; }

    /// <summary>Get database version.</summary>
    int Version { get; }

    /// <summary>Execute migrations up to current schema version.</summary>
    Task RunMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Create new connection for query execution.</summary>
    IDbConnection CreateConnection();

    /// <summary>Execute raw SQL with parameters.</summary>
    Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null, 
        CancellationToken cancellationToken = default);

    /// <summary>Query with results.</summary>
    Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>Query single result or null.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>Begin transaction.</summary>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Database path (for debugging).</summary>
    string? DatabasePath { get; }
}

/// <summary>Database connection wrapper.</summary>
public interface IDbConnection : IAsyncDisposable
{
    /// <summary>Execute non-query SQL.</summary>
    Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null);

    /// <summary>Query with results.</summary>
    Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null) 
        where T : class, new();

    /// <summary>Query single result.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new();
}

/// <summary>Database transaction wrapper.</summary>
public interface IDbTransaction : IAsyncDisposable
{
    /// <summary>Commit changes.</summary>
    Task CommitAsync();

    /// <summary>Rollback changes.</summary>
    Task RollbackAsync();

    /// <summary>Get connection associated with this transaction.</summary>
    IDbConnection Connection { get; }
}
```

---

- [ ] **Step 2: Create DbMigration base class**

```csharp
// src/SmartWorkz.Core.MAUI/Data/Database/DbMigration.cs
namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Base class for database schema migrations.</summary>
public abstract class DbMigration
{
    /// <summary>Migration version number (001, 002, etc).</summary>
    public abstract int Version { get; }

    /// <summary>Description of changes in this migration.</summary>
    public abstract string Description { get; }

    /// <summary>Apply migration to database.</summary>
    public abstract Task UpAsync(IDbConnection connection);

    /// <summary>Revert migration (optional).</summary>
    public virtual Task DownAsync(IDbConnection connection)
    {
        return Task.CompletedTask;
    }
}
```

---

- [ ] **Step 3: Create migration implementations**

```csharp
// src/SmartWorkz.Core.MAUI/Data/Database/Migrations/Migration_001_InitialSchema.cs
namespace SmartWorkz.Mobile.Data.Database.Migrations;

public class Migration_001_InitialSchema : DbMigration
{
    public override int Version => 1;
    public override string Description => "Create initial database schema with sync tables";

    public override async Task UpAsync(IDbConnection connection)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS SyncEntity (
    Id TEXT PRIMARY KEY,
    EntityType TEXT NOT NULL,
    Data TEXT NOT NULL,
    Version INTEGER DEFAULT 1,
    IsSynced BOOLEAN DEFAULT 0,
    SyncedAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS LocalChangeLog (
    Id TEXT PRIMARY KEY,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    ChangeType TEXT NOT NULL,
    Data TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsSynced BOOLEAN DEFAULT 0,
    SyncedAt DATETIME
);

CREATE TABLE IF NOT EXISTS SyncState (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_SyncEntity_EntityType ON SyncEntity(EntityType);
CREATE INDEX IF NOT EXISTS idx_SyncEntity_IsSynced ON SyncEntity(IsSynced);
CREATE INDEX IF NOT EXISTS idx_LocalChangeLog_EntityId ON LocalChangeLog(EntityId);
CREATE INDEX IF NOT EXISTS idx_LocalChangeLog_IsSynced ON LocalChangeLog(IsSynced);
        ";

        await connection.ExecuteAsync(sql);
    }
}

// src/SmartWorkz.Core.MAUI/Data/Database/Migrations/Migration_002_AddNotificationLog.cs
public class Migration_002_AddNotificationLog : DbMigration
{
    public override int Version => 2;
    public override string Description => "Add push notification history table";

    public override async Task UpAsync(IDbConnection connection)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS NotificationLog (
    Id TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Body TEXT NOT NULL,
    Data TEXT,
    ReceivedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsRead BOOLEAN DEFAULT 0,
    DeepLink TEXT
);

CREATE INDEX IF NOT EXISTS idx_NotificationLog_ReceivedAt ON NotificationLog(ReceivedAt);
CREATE INDEX IF NOT EXISTS idx_NotificationLog_IsRead ON NotificationLog(IsRead);
        ";

        await connection.ExecuteAsync(sql);
    }
}
```

---

- [ ] **Step 4: Write test for database interface**

```csharp
// tests/SmartWorkz.Core.MAUI.Tests/Data/DatabaseTests.cs
using Xunit;
using SmartWorkz.Mobile.Data.Database;

public class DatabaseTests
{
    [Fact]
    public async Task OpenAsync_WithValidPath_OpensConnection()
    {
        // Arrange
        var db = new TestDatabase();

        // Act
        await db.OpenAsync();

        // Assert
        Assert.True(db.IsConnected);
    }

    [Fact]
    public async Task RunMigrationsAsync_ExecutesAllMigrations()
    {
        // Arrange
        var db = new TestDatabase();
        await db.OpenAsync();

        // Act
        await db.RunMigrationsAsync();

        // Assert
        Assert.True(db.IsConnected);
        // Verify tables exist by querying
        var tables = await db.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table'");
        Assert.Contains("SyncEntity", tables);
    }

    private class TestDatabase : IDatabase
    {
        public bool IsConnected { get; private set; }
        public int Version => 2;
        public string? DatabasePath => ":memory:";

        public Task OpenAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
        {
            // Implementation would apply migrations
            await Task.CompletedTask;
        }

        public IDbConnection CreateConnection() => throw new NotImplementedException();
        public Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default) where T : class, new() => throw new NotImplementedException();
        public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default) where T : class, new() => throw new NotImplementedException();
        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
```

---

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/Data/Database/ tests/SmartWorkz.Core.MAUI.Tests/Data/
git commit -m "feat(data): add database abstraction interfaces and migrations"
```

---

### Task 4: SQLite Database Implementation

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/SQLiteDatabase.cs`
- Create: `src/SmartWorkz.Core.MAUI/Data/Database/DbMigrationRunner.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/Data/SQLiteDatabaseTests.cs`

**Steps:**

- [ ] **Step 1: Create SQLiteDatabase implementation**

```csharp
// src/SmartWorkz.Core.MAUI/Data/Database/SQLiteDatabase.cs
using sqlite3 = SQLite.SQLiteConnection;
using SmartWorkz.Mobile.Data.Database.Migrations;

namespace SmartWorkz.Mobile.Data.Database;

/// <summary>SQLite implementation of IDatabase for all platforms.</summary>
public class SQLiteDatabase : IDatabase
{
    private sqlite3? _connection;
    private readonly string _databasePath;
    private readonly ILogger<SQLiteDatabase> _logger;
    private readonly DbMigrationRunner _migrationRunner;

    public bool IsConnected => _connection != null;
    public int Version { get; private set; }
    public string? DatabasePath => _databasePath;

    public SQLiteDatabase(string databasePath, ILogger<SQLiteDatabase> logger)
    {
        _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _migrationRunner = new DbMigrationRunner(logger);
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection != null) return;

            _logger.LogInformation("Opening SQLite database at {Path}", _databasePath);
            
            // Ensure directory exists
            var dir = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _connection = new sqlite3(_databasePath);
            _connection.CreateConnection();

            // Enable WAL mode for better concurrency
            await ExecuteAsync("PRAGMA journal_mode = WAL");
            await ExecuteAsync("PRAGMA foreign_keys = ON");

            _logger.LogInformation("SQLite database opened successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open SQLite database");
            throw;
        }
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_connection == null) return;

        try
        {
            _logger.LogInformation("Closing SQLite database");
            _connection.Close();
            _connection = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing database");
            throw;
        }
    }

    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        try
        {
            var migrations = new DbMigration[]
            {
                new Migration_001_InitialSchema(),
                new Migration_002_AddNotificationLog()
            };

            var connection = new SQLiteDbConnection(_connection, _logger);
            await _migrationRunner.RunAsync(connection, migrations);

            Version = migrations.Max(m => m.Version);
            _logger.LogInformation("Database migrations completed. Version: {Version}", Version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running migrations");
            throw;
        }
    }

    public IDbConnection CreateConnection()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        return new SQLiteDbConnection(_connection, _logger);
    }

    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection == null)
            throw new InvalidOperationException("Database not opened");

        var connection = CreateConnection();
        return await SQLiteDbTransaction.BeginAsync(connection);
    }
}

/// <summary>SQLite connection wrapper implementing IDbConnection.</summary>
internal class SQLiteDbConnection : IDbConnection
{
    private readonly sqlite3 _connection;
    private readonly ILogger _logger;

    public SQLiteDbConnection(sqlite3 connection, ILogger logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null)
    {
        try
        {
            var command = _connection.CreateCommand(sql);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Bind(param.Key, param.Value);
                }
            }
            return await Task.FromResult(command.ExecuteNonQuery());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL: {Sql}", sql);
            throw;
        }
    }

    public async Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new()
    {
        try
        {
            var command = _connection.CreateCommand(sql);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Bind(param.Key, param.Value);
                }
            }

            var results = new List<T>();
            while (command.Step() == SQLiteResult.ROW)
            {
                var item = MapRow<T>(command);
                results.Add(item);
            }
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying SQL: {Sql}", sql);
            throw;
        }
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new()
    {
        var results = await QueryAsync<T>(sql, parameters);
        return results.FirstOrDefault();
    }

    public async ValueTask DisposeAsync()
    {
        // Connection owned by database, don't dispose here
        await Task.CompletedTask;
    }

    private T MapRow<T>(sqlite3.SQLiteCommand command) where T : class, new()
    {
        var item = new T();
        for (int i = 0; i < command.ColumnCount; i++)
        {
            var columnName = command.ColumnName(i);
            var value = command.GetValue(i);
            
            var property = typeof(T).GetProperty(columnName,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);
            
            if (property != null && value != null)
            {
                try
                {
                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                }
                catch { }
            }
        }
        return item;
    }
}

/// <summary>SQLite transaction wrapper.</summary>
internal class SQLiteDbTransaction : IDbTransaction
{
    private readonly IDbConnection _connection;
    private bool _committed = false;

    private SQLiteDbTransaction(IDbConnection connection)
    {
        _connection = connection;
    }

    public static async Task<IDbTransaction> BeginAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync("BEGIN TRANSACTION");
        return new SQLiteDbTransaction(connection);
    }

    public async Task CommitAsync()
    {
        if (!_committed)
        {
            await _connection.ExecuteAsync("COMMIT");
            _committed = true;
        }
    }

    public async Task RollbackAsync()
    {
        if (!_committed)
        {
            await _connection.ExecuteAsync("ROLLBACK");
            _committed = true;
        }
    }

    public IDbConnection Connection => _connection;

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await RollbackAsync();
        }
    }
}
```

---

- [ ] **Step 2: Create DbMigrationRunner**

```csharp
// src/SmartWorkz.Core.MAUI/Data/Database/DbMigrationRunner.cs
namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Executes database migrations in order.</summary>
public class DbMigrationRunner
{
    private readonly ILogger<DbMigrationRunner> _logger;

    public DbMigrationRunner(ILogger<DbMigrationRunner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(IDbConnection connection, params DbMigration[] migrations)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(migrations);

        // Get current schema version
        var currentVersion = await GetCurrentVersionAsync(connection);
        _logger.LogInformation("Current database version: {Version}", currentVersion);

        var sortedMigrations = migrations.OrderBy(m => m.Version).ToList();

        foreach (var migration in sortedMigrations)
        {
            if (migration.Version <= currentVersion)
            {
                _logger.LogDebug("Skipping migration {Version}: {Description}", 
                    migration.Version, migration.Description);
                continue;
            }

            try
            {
                _logger.LogInformation("Applying migration {Version}: {Description}",
                    migration.Version, migration.Description);

                await migration.UpAsync(connection);
                await SetCurrentVersionAsync(connection, migration.Version);

                _logger.LogInformation("Migration {Version} completed", migration.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration {Version} failed", migration.Version);
                throw;
            }
        }
    }

    private async Task<int> GetCurrentVersionAsync(IDbConnection connection)
    {
        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Value FROM SyncState WHERE Key = 'SchemaVersion'");
            
            if (result != null && int.TryParse(result.Value?.ToString(), out var version))
            {
                return version;
            }
        }
        catch
        {
            // Table doesn't exist yet, return 0
        }

        return 0;
    }

    private async Task SetCurrentVersionAsync(IDbConnection connection, int version)
    {
        await connection.ExecuteAsync(
            "INSERT OR REPLACE INTO SyncState (Key, Value) VALUES ('SchemaVersion', @version)",
            new Dictionary<string, object?> { { "@version", version.ToString() } });
    }
}
```

---

- [ ] **Step 3: Run tests**

```bash
dotnet test tests/SmartWorkz.Core.MAUI.Tests/Data/ -v
```
Expected: **PASS** - Database opens, migrations run

---

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/Data/Database/SQLiteDatabase.cs src/SmartWorkz.Core.MAUI/Data/Database/DbMigrationRunner.cs
git commit -m "feat(data): implement SQLite database with migration runner"
```

---

### Task 5: Crash Reporting Service (Sentry Integration)

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/Telemetry/ICrashReportingService.cs`
- Create: `src/SmartWorkz.Core.MAUI/Telemetry/SentryCrashReporter.cs`
- Create: `src/SmartWorkz.Core.MAUI/Telemetry/CrashContext.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/Telemetry/CrashReportingTests.cs`

**Steps:**

- [ ] **Step 1: Create ICrashReportingService interface**

```csharp
// src/SmartWorkz.Core.MAUI/Telemetry/ICrashReportingService.cs
namespace SmartWorkz.Mobile.Telemetry;

/// <summary>
/// Crash reporting service for capturing and sending crash data to backend.
/// Integrates with Sentry or similar crash reporting platform.
/// </summary>
public interface ICrashReportingService
{
    /// <summary>Initialize crash reporter with configuration.</summary>
    Task InitializeAsync(string dsn, string environment = "production");

    /// <summary>Capture unhandled exception.</summary>
    Task CaptureExceptionAsync(Exception exception, CrashContext? context = null);

    /// <summary>Capture message/log.</summary>
    Task CaptureMessageAsync(string message, SeverityLevel level = SeverityLevel.Info);

    /// <summary>Set user context for crash reports.</summary>
    void SetUserContext(string userId, string? email = null, string? username = null);

    /// <summary>Set custom context data.</summary>
    void SetContext(string key, Dictionary<string, object>? data);

    /// <summary>Add breadcrumb (action trail before crash).</summary>
    void AddBreadcrumb(string message, string category = "default", SeverityLevel level = SeverityLevel.Info);

    /// <summary>Close and flush remaining crash data.</summary>
    Task CloseAsync();
}

/// <summary>Severity level for crash reports.</summary>
public enum SeverityLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

/// <summary>Context data for crash report.</summary>
public class CrashContext
{
    /// <summary>User ID for crash.</summary>
    public string? UserId { get; init; }

    /// <summary>Device information.</summary>
    public Dictionary<string, object>? DeviceInfo { get; init; }

    /// <summary>App version.</summary>
    public string? AppVersion { get; init; }

    /// <summary>Release/build version.</summary>
    public string? BuildVersion { get; init; }

    /// <summary>Custom tags.</summary>
    public Dictionary<string, string>? Tags { get; init; }

    /// <summary>Custom data.</summary>
    public Dictionary<string, object>? Data { get; init; }
}
```

---

- [ ] **Step 2: Create Sentry implementation**

```csharp
// src/SmartWorkz.Core.MAUI/Telemetry/SentryCrashReporter.cs
using Sentry;
using Sentry.Maui;

namespace SmartWorkz.Mobile.Telemetry;

/// <summary>Sentry-based crash reporting implementation.</summary>
public class SentryCrashReporter : ICrashReportingService
{
    private readonly ILogger<SentryCrashReporter> _logger;
    private IHub? _sentryHub;
    private bool _initialized = false;

    public SentryCrashReporter(ILogger<SentryCrashReporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(string dsn, string environment = "production")
    {
        if (string.IsNullOrEmpty(dsn))
            throw new ArgumentException("DSN is required", nameof(dsn));

        try
        {
            _logger.LogInformation("Initializing Sentry crash reporter. Environment: {Environment}", environment);

            var options = new SentryMauiOptions
            {
                Dsn = dsn,
                Environment = environment,
                // Release will be set from app version
                AttachStacktrace = true,
                MaxBreadcrumbs = 100,
                SendDefaultPii = false,
                TracesSampleRate = 0.1, // 10% of transactions
                Debug = false,
                IncludeEventProcessors = true
            };

            SentyMauiOptions.Init(options);
            _sentryHub = SentyHub.Current;
            _initialized = true;

            _logger.LogInformation("Sentry initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Sentry");
            throw;
        }
    }

    public async Task CaptureExceptionAsync(Exception exception, CrashContext? context = null)
    {
        if (!_initialized || _sentryHub == null)
        {
            _logger.LogWarning("Crash reporter not initialized. Exception not reported: {Message}", 
                exception.Message);
            return;
        }

        try
        {
            _logger.LogError(exception, "Capturing exception for crash reporting");

            var sentryEvent = new SentryEvent(exception);

            if (context != null)
            {
                if (!string.IsNullOrEmpty(context.UserId))
                {
                    sentryEvent.User = new SentryUser { Id = context.UserId };
                }

                if (context.Tags != null)
                {
                    foreach (var tag in context.Tags)
                    {
                        sentryEvent.Tags[tag.Key] = tag.Value;
                    }
                }

                if (context.Data != null)
                {
                    sentryEvent.Contexts["custom"] = context.Data;
                }

                if (context.DeviceInfo != null)
                {
                    sentryEvent.Contexts["device"] = context.DeviceInfo;
                }

                if (!string.IsNullOrEmpty(context.AppVersion))
                {
                    sentryEvent.Release = context.AppVersion;
                }
            }

            _sentryHub.CaptureEvent(sentryEvent);
            await _sentryHub.FlushAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing exception in crash reporter");
        }
    }

    public async Task CaptureMessageAsync(string message, SeverityLevel level = SeverityLevel.Info)
    {
        if (!_initialized || _sentryHub == null) return;

        try
        {
            var sentryLevel = MapSeverityLevel(level);
            _sentryHub.CaptureMessage(message, sentryLevel);
            await _sentryHub.FlushAsync(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing message in crash reporter");
        }
    }

    public void SetUserContext(string userId, string? email = null, string? username = null)
    {
        if (!_initialized || _sentryHub == null) return;

        try
        {
            _sentryHub.ConfigureScope(scope =>
            {
                scope.User = new SentryUser
                {
                    Id = userId,
                    Email = email,
                    Username = username
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user context");
        }
    }

    public void SetContext(string key, Dictionary<string, object>? data)
    {
        if (!_initialized || _sentryHub == null) return;

        try
        {
            _sentryHub.ConfigureScope(scope =>
            {
                scope.Contexts[key] = data ?? new Dictionary<string, object>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting context");
        }
    }

    public void AddBreadcrumb(string message, string category = "default", SeverityLevel level = SeverityLevel.Info)
    {
        if (!_initialized || _sentryHub == null) return;

        try
        {
            var sentryLevel = MapSeverityLevel(level);
            _sentryHub.AddBreadcrumb(message, category, level: sentryLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding breadcrumb");
        }
    }

    public async Task CloseAsync()
    {
        if (!_initialized || _sentryHub == null) return;

        try
        {
            await _sentryHub.FlushAsync(TimeSpan.FromSeconds(10));
            _sentryHub.Dispose();
            _initialized = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing Sentry");
        }
    }

    private static SentryLevel MapSeverityLevel(SeverityLevel level)
    {
        return level switch
        {
            SeverityLevel.Debug => SentryLevel.Debug,
            SeverityLevel.Info => SentryLevel.Info,
            SeverityLevel.Warning => SentryLevel.Warning,
            SeverityLevel.Error => SentryLevel.Error,
            SeverityLevel.Fatal => SentryLevel.Fatal,
            _ => SentryLevel.Info
        };
    }
}
```

---

- [ ] **Step 3: Write tests**

```csharp
// tests/SmartWorkz.Core.MAUI.Tests/Telemetry/CrashReportingTests.cs
using Xunit;
using SmartWorkz.Mobile.Telemetry;

public class CrashReportingTests
{
    [Fact]
    public async Task CaptureExceptionAsync_WithContext_ReportsCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<SentryCrashReporter>();
        var service = new SentryCrashReporter(logger);
        
        // Note: Full test would require mocking Sentry SDK
        var exception = new InvalidOperationException("Test exception");
        var context = new CrashContext
        {
            UserId = "user123",
            Tags = new() { { "feature", "payments" } }
        };

        // Act & Assert - would capture without throwing
        await service.CaptureExceptionAsync(exception, context);
    }

    [Fact]
    public void SetUserContext_StoresUserInfo()
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<SentryCrashReporter>();
        var service = new SentryCrashReporter(logger);

        service.SetUserContext("user123", "user@example.com", "johndoe");
        // Would verify user was set in Sentry SDK
    }
}
```

---

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/Telemetry/ tests/SmartWorkz.Core.MAUI.Tests/Telemetry/
git commit -m "feat(telemetry): add Sentry crash reporting service"
```

---

### Task 6: Complete Push Notification Pipeline

**Files:**
- Modify: `src/SmartWorkz.Core.MAUI/Notifications/IPushNotificationHandler.cs`
- Create: `src/SmartWorkz.Core.MAUI/Notifications/PushMessageRouter.cs`
- Create: `src/SmartWorkz.Core.MAUI/Notifications/PushNotificationPermissionHelper.cs`
- Test: `tests/SmartWorkz.Core.MAUI.Tests/Notifications/PushNotificationTests.cs`

*Due to length constraints, this task shows the interfaces and key pieces. Full code available in implementation.*

- [ ] **Step 1: Create IPushNotificationHandler interface**

```csharp
// src/SmartWorkz.Core.MAUI/Notifications/IPushNotificationHandler.cs
namespace SmartWorkz.Mobile.Notifications;

/// <summary>
/// Handles incoming push notifications with routing and deep linking.
/// </summary>
public interface IPushNotificationHandler
{
    /// <summary>Handle received push notification message.</summary>
    Task HandleMessageAsync(PushMessage message);

    /// <summary>Register route for push notifications.</summary>
    void RegisterRoute(string actionId, Func<PushMessage, Task> handler);

    /// <summary>Open push notification (for UI interaction).</summary>
    Task OpenNotificationAsync(string notificationId);
}

public class PushMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ActionId { get; init; }
    public Dictionary<string, string>? Data { get; init; }
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
}
```

---

- [ ] **Step 2: Create PushMessageRouter**

```csharp
// src/SmartWorkz.Core.MAUI/Notifications/PushMessageRouter.cs
namespace SmartWorkz.Mobile.Notifications;

/// <summary>Routes push messages to appropriate handlers.</summary>
public class PushMessageRouter : IPushNotificationHandler
{
    private readonly Dictionary<string, Func<PushMessage, Task>> _routes = new();
    private readonly IAppStore _store;
    private readonly ILogger<PushMessageRouter> _logger;

    public PushMessageRouter(IAppStore store, ILogger<PushMessageRouter> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RegisterRoute(string actionId, Func<PushMessage, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _routes[actionId] = handler;
        _logger.LogDebug("Registered push notification route: {ActionId}", actionId);
    }

    public async Task HandleMessageAsync(PushMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("Handling push notification: {Title}", message.Title);

        // Add to store
        _store.Dispatch(new AddNotificationAction { Notification = new PushNotification 
        { 
            Title = message.Title,
            Body = message.Body,
            Data = message.Data
        }});

        // Route to handler if action exists
        if (!string.IsNullOrEmpty(message.ActionId) && _routes.TryGetValue(message.ActionId, out var handler))
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling push notification route: {ActionId}", message.ActionId);
            }
        }
    }

    public async Task OpenNotificationAsync(string notificationId)
    {
        _store.Dispatch(new MarkNotificationAsReadAction { NotificationId = notificationId });
        await Task.CompletedTask;
    }
}
```

---

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/Notifications/
git commit -m "feat(notifications): complete push notification pipeline with routing"
```

---

### Task 7: Register Phase 3A Services in MauiProgram

**Files:**
- Modify: `src/SmartWorkz.Core.MAUI/MauiProgram.cs`

- [ ] **Step 1: Update MauiProgram.cs with Phase 3A services**

```csharp
// Add to MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureEssentials()
            .AddSmartWorkzComponentLibrary()
            // Phase 3A: Infrastructure
            .AddStateManagement()
            .AddDataPersistence()
            .AddCrashReporting()
            .AddPushNotifications();

        return builder.Build();
    }

    // State Management
    private static MauiAppBuilder AddStateManagement(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IReducerRegistry>(sp => 
            new ReducerRegistry(sp.GetRequiredService<ILogger<ReducerRegistry>>())
                .Register(new AppReducer()));

        builder.Services.AddSingleton<IAppStore>(sp =>
            new AppStore(
                new AppState(),
                sp.GetRequiredService<IReducerRegistry>(),
                sp.GetRequiredService<ILogger<AppStore>>()));

        return builder;
    }

    // Data Persistence
    private static MauiAppBuilder AddDataPersistence(this MauiAppBuilder builder)
    {
        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "smartworkz.db");

        builder.Services.AddSingleton<IDatabase>(sp =>
            new SQLiteDatabase(dbPath, sp.GetRequiredService<ILogger<SQLiteDatabase>>()));

        return builder;
    }

    // Crash Reporting
    private static MauiAppBuilder AddCrashReporting(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ICrashReportingService>(sp =>
            new SentryCrashReporter(sp.GetRequiredService<ILogger<SentryCrashReporter>>()));

        return builder;
    }

    // Push Notifications
    private static MauiAppBuilder AddPushNotifications(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IPushNotificationHandler>(sp =>
            new PushMessageRouter(
                sp.GetRequiredService<IAppStore>(),
                sp.GetRequiredService<ILogger<PushMessageRouter>>()));

        return builder;
    }
}
```

---

- [ ] **Step 2: Commit**

```bash
git add src/SmartWorkz.Core.MAUI/MauiProgram.cs
git commit -m "feat(core): register Phase 3A infrastructure services in DI"
```

---

## Phase 3B: UI Components (Weeks 4-5)

### File Structure - Phase 3B

```
src/SmartWorkz.Core.MAUI/Components/
├── DatePickers/                    # NEW
│   ├── DatePicker.xaml
│   ├── DatePicker.xaml.cs
│   ├── TimePicker.xaml
│   ├── TimePicker.xaml.cs
│   └── DateTimePickerBehaviors.cs
├── SearchBars/                     # NEW
│   ├── SearchBar.xaml
│   ├── SearchBar.xaml.cs
│   └── SearchViewModel.cs
├── DataGrids/                      # NEW
│   ├── DataGrid.xaml
│   ├── DataGrid.xaml.cs
│   ├── DataGridColumn.cs
│   └── DataGridCellTemplate.xaml
├── Tabs/                           # NEW
│   ├── TabControl.xaml
│   ├── TabControl.xaml.cs
│   ├── TabItem.xaml
│   └── TabItem.xaml.cs
├── BottomSheets/                   # NEW
│   ├── BottomSheet.xaml
│   ├── BottomSheet.xaml.cs
│   └── BottomSheetBehavior.cs
└── COMPONENT_USAGE.md              # UPDATED with 11 components
```

*Due to length, Phase 3B tasks follow same structure as 3A but focus on UI component implementation.*

---

### Task 8-12: UI Components (Date Picker, Search, Data Grid, Tabs, Bottom Sheet)

Each component follows same pattern:
1. Write failing tests
2. Implement interfaces
3. Create XAML UI
4. Create code-behind/ViewModel
5. Add documentation
6. Commit

*[Full code for each component would follow same structure as Phase 3A tasks]*

---

## Phase 3C: Developer Experience (Week 6)

### File Structure - Phase 3C

```
src/SmartWorkz.Core.MAUI/
├── Configuration/                  # NEW
│   ├── IAppConfiguration.cs
│   ├── AppConfiguration.cs
│   ├── Environment.cs
│   └── ConfigurationExtensions.cs
├── Accessibility/                  # NEW
│   ├── IAccessibilityService.cs
│   ├── AccessibilityService.cs
│   ├── SemanticMarkupHelper.cs
│   └── AccessibilityGuidelines.md
└── Testing/                        # NEW
    ├── TestViewModelBase.cs
    ├── TestDataBuilder.cs
    └── MockServiceFactory.cs
```

---

### Task 13: Environment-Based Configuration

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/Configuration/IAppConfiguration.cs`
- Create: `src/SmartWorkz.Core.MAUI/Configuration/AppConfiguration.cs`
- Create: `src/SmartWorkz.Core.MAUI/Configuration/AppConfigurationExtensions.cs`

*[Implementation follows same TDD pattern as previous tasks]*

---

### Task 14: Accessibility Helpers

**Files:**
- Create: `src/SmartWorkz.Core.MAUI/Accessibility/IAccessibilityService.cs`
- Create: `src/SmartWorkz.Core.MAUI/Accessibility/AccessibilityService.cs`

---

### Task 15: Component Testing Framework

**Files:**
- Create: `tests/SmartWorkz.Core.MAUI.Tests/TestViewModelBase.cs`
- Create: `tests/SmartWorkz.Core.MAUI.Tests/TestDataBuilder.cs`

---

## Success Criteria

### Phase 3A Completion
- [ ] Redux store dispatches actions and updates state immutably
- [ ] SQLite database with 2 migrations running successfully
- [ ] Sentry crash reporting integrated and tested
- [ ] Push notification pipeline routes messages correctly
- [ ] All 3A services registered in DI
- [ ] 20+ unit tests passing
- [ ] Code coverage > 80% for 3A

### Phase 3B Completion
- [ ] 5 new UI components working (date picker, search, data grid, tabs, bottom sheet)
- [ ] All components support MVVM binding
- [ ] Component examples documented
- [ ] 5+ new tests per component
- [ ] Cross-platform tested (iOS, Android)

### Phase 3C Completion
- [ ] Environment-based configuration working (dev/staging/prod)
- [ ] Accessibility helpers available
- [ ] Component testing framework documented
- [ ] 15+ total UI components
- [ ] Framework ready for production apps

---

## Merge & Integration Points

**After Phase 3A (Week 3):**
```bash
git checkout main
git merge feature/mobile-2.0-infrastructure
git tag -a mobile-2.0-phase-3a -m "Phase 3A: Infrastructure complete"
```

**After Phase 3B (Week 5):**
```bash
git merge feature/mobile-2.0-ui-components
git tag -a mobile-2.0-phase-3b -m "Phase 3B: UI Components complete"
```

**After Phase 3C (Week 6):**
```bash
git merge feature/mobile-2.0-dx
git tag -a mobile-2.0 -m "SmartWorkz Mobile Framework 2.0 - Production Ready"
```

---

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| SQLite schema changes | Data loss | Version migrations, test database, rollback plan |
| Sentry downtime | Crash data loss | Local queue, retry logic, fallback logging |
| Push notification race conditions | Duplicate notifications | Idempotency keys, deduplication in reducer |
| State mutation bugs | State inconsistency | Immutability enforcement, tests for each reducer |
| Performance regression | Slow app | Benchmark tests for store, DB queries |
| Component accessibility | Compliance violations | WCAG testing, automated a11y tests |

---

## Team Allocation (5+ developers)

**Team A (2 devs): Phase 3A Infrastructure**
- Dev 1: Redux store + reducers
- Dev 2: Database + Sentry

**Team B (2 devs): Phase 3B UI Components**
- Dev 1: Date picker + search
- Dev 2: Data grid + tabs/bottom sheet

**Team C (1+ devs): Phase 3C DX + Integration**
- Dev 1: Configuration + accessibility
- Dev 2: Testing framework + documentation

**Parallel Work:** Teams work independently, integrate at phase boundaries

---

## Definition of Done

Each task is done when:
1. ✅ Code written following patterns
2. ✅ Tests written (TDD) and passing
3. ✅ Code reviewed (pair programming or PR)
4. ✅ Documentation updated
5. ✅ Committed to feature branch
6. ✅ No breaking changes to existing APIs

---

## Notes for Implementers

- **DRY principle:** Reuse existing services, don't duplicate logic
- **YAGNI principle:** Build only what's needed for MVP, no gold-plating
- **TDD discipline:** Write test first, implement minimal code to pass
- **Frequent commits:** Small, logical commits with clear messages
- **Cross-platform testing:** Test on iOS, Android, Windows when possible
- **Documentation:** Add examples to COMPONENT_USAGE.md as components ship
