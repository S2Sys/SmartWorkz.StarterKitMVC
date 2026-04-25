using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Reducers;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
