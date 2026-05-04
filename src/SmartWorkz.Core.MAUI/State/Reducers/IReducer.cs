using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Interface for Redux reducers that handle state transitions.</summary>
public interface IReducer
{
    /// <summary>Gets the set of action types this reducer handles.</summary>
    IEnumerable<Type> HandledActionTypes { get; }

    /// <summary>
    /// Apply an action to the current state and return a new state.
    /// Pure function - must not modify input state.
    /// </summary>
    /// <param name="state">Current application state.</param>
    /// <param name="action">Action to apply.</param>
    /// <returns>New state after applying the action, or unchanged state if action not handled.</returns>
    AppState Reduce(AppState state, IAction action);
}
