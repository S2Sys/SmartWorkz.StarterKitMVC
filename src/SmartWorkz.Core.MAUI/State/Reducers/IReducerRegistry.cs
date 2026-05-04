using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Registry for reducer functions that process actions to update state.</summary>
public interface IReducerRegistry
{
    /// <summary>Reduce the current state by applying an action.</summary>
    /// <param name="state">Current application state.</param>
    /// <param name="action">Action to apply.</param>
    /// <returns>New state after applying the action.</returns>
    AppState Reduce(AppState state, IAction action);
}
