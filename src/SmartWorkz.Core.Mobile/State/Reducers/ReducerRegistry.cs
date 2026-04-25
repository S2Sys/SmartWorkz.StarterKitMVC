using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Default implementation of the reducer registry.</summary>
public class ReducerRegistry : IReducerRegistry
{
    private readonly Dictionary<Type, Func<AppState, IAction, AppState>> _reducers = new();

    /// <summary>Register a reducer function for a specific action type.</summary>
    public void Register<TAction>(Func<AppState, TAction, AppState> reducer)
        where TAction : IAction
    {
        // Cast to the non-generic delegate type for storage
        _reducers[typeof(TAction)] = (state, action) => reducer(state, (TAction)action);
    }

    /// <summary>Reduce the current state by applying an action.</summary>
    public AppState Reduce(AppState state, IAction action)
    {
        var actionType = action.GetType();

        if (_reducers.TryGetValue(actionType, out var reducer))
        {
            return reducer(state, action);
        }

        // If no reducer registered, return unchanged state
        return state;
    }
}
