namespace SmartWorkz.Mobile.State.Store;

using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Reducers;

/// <summary>
/// Redux store implementation managing application state.
/// Provides immutable state updates via pure reducer functions.
/// Thread-safe with subscriber notifications on state changes.
/// </summary>
public class AppStore : IAppStore
{
    private AppState _state = new();
    private readonly object _stateLock = new();
    private readonly List<(Action<AppState> callback, Guid id)> _subscribers = new();
    private readonly AppReducer _reducer = new();
    private readonly object _subscribersLock = new();

    public AppState State
    {
        get
        {
            lock (_stateLock)
            {
                return _state;
            }
        }
    }

    public int SubscriberCount
    {
        get
        {
            lock (_subscribersLock)
            {
                return _subscribers.Count;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the AppStore with initial state.
    /// </summary>
    public AppStore()
    {
        _state = new AppState();
    }

    /// <summary>
    /// Dispatches an action to update state.
    /// Calls the reducer with current state and action to compute new state,
    /// then notifies all subscribers.
    /// </summary>
    /// <param name="action">The action to dispatch.</param>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public void Dispatch(IAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        AppState newState;

        lock (_stateLock)
        {
            newState = _reducer.Reduce(_state, action);

            // Only update if state actually changed (reference or value equality)
            if (ReferenceEquals(_state, newState) || _state == newState)
            {
                return;
            }

            _state = newState;
        }

        // Notify subscribers outside of lock to prevent deadlocks
        NotifySubscribers(newState);
    }

    /// <summary>
    /// Subscribes to all state changes.
    /// </summary>
    /// <param name="callback">Called with the new state whenever it changes.</param>
    /// <returns>A disposable that removes the subscription when disposed.</returns>
    public IDisposable Subscribe(Action<AppState> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var id = Guid.NewGuid();

        lock (_subscribersLock)
        {
            _subscribers.Add((callback, id));
        }

        // Return a disposable that unsubscribes
        return new Unsubscriber(this, id);
    }

    /// <summary>
    /// Subscribes to changes of a specific state slice.
    /// Only calls the callback when the selected value changes.
    /// </summary>
    /// <typeparam name="T">The type of the state slice.</typeparam>
    /// <param name="selector">Extracts a portion of state.</param>
    /// <param name="callback">Called when the selected portion changes.</param>
    /// <returns>A disposable that removes the subscription when disposed.</returns>
    public IDisposable SubscribeToSlice<T>(Func<AppState, T> selector, Action<T> callback)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(callback);

        T? previousValue = default;
        var isFirstCall = true;

        return Subscribe(newState =>
        {
            var currentValue = selector(newState);

            if (isFirstCall || !EqualityComparer<T>.Default.Equals(currentValue, previousValue))
            {
                isFirstCall = false;
                previousValue = currentValue;
                callback(currentValue);
            }
        });
    }

    /// <summary>
    /// Notifies all subscribers of a state change.
    /// </summary>
    private void NotifySubscribers(AppState newState)
    {
        List<(Action<AppState> callback, Guid id)> subscribersCopy;

        lock (_subscribersLock)
        {
            subscribersCopy = new List<(Action<AppState>, Guid)>(_subscribers);
        }

        foreach (var (callback, _) in subscribersCopy)
        {
            try
            {
                callback(newState);
            }
            catch
            {
                // Silently ignore subscriber errors to prevent one bad subscriber from affecting others
            }
        }
    }

    /// <summary>
    /// Internal helper class for unsubscribing.
    /// </summary>
    private class Unsubscriber : IDisposable
    {
        private readonly AppStore _store;
        private readonly Guid _id;
        private bool _disposed;

        public Unsubscriber(AppStore store, Guid id)
        {
            _store = store;
            _id = id;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_store._subscribersLock)
                {
                    _store._subscribers.RemoveAll(s => s.id == _id);
                }

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
