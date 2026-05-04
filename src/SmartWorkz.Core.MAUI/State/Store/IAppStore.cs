namespace SmartWorkz.Mobile.State.Store;

using SmartWorkz.Mobile.State.Actions;

/// <summary>
/// Redux state store interface for managing application state.
/// Provides dispatch, subscribe, and state access capabilities.
/// </summary>
public interface IAppStore
{
    /// <summary>
    /// Gets the current application state (immutable snapshot).
    /// </summary>
    AppState State { get; }

    /// <summary>
    /// Dispatches an action to the store, triggering state reduction and subscribers.
    /// </summary>
    /// <param name="action">The action to dispatch.</param>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    void Dispatch(IAction action);

    /// <summary>
    /// Subscribes to state changes. Callback is invoked whenever the state changes.
    /// </summary>
    /// <param name="callback">Callback invoked when state changes with the new state.</param>
    /// <returns>An IDisposable that unsubscribes when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
    IDisposable Subscribe(Action<AppState> callback);

    /// <summary>
    /// Subscribes to changes of a specific slice of state.
    /// Callback is invoked only when the selected state slice changes.
    /// </summary>
    /// <typeparam name="T">The type of the state slice.</typeparam>
    /// <param name="selector">Function to select a portion of the state.</param>
    /// <param name="callback">Callback invoked when the selected state changes.</param>
    /// <returns>An IDisposable that unsubscribes when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when selector or callback is null.</exception>
    IDisposable SubscribeToSlice<T>(Func<AppState, T> selector, Action<T> callback);

    /// <summary>
    /// Gets the number of active subscribers.
    /// </summary>
    int SubscriberCount { get; }
}
