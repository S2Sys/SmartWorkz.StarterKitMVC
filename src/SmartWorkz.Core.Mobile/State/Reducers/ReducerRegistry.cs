using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.State.Reducers;

/// <summary>Default implementation of the reducer registry.</summary>
public class ReducerRegistry : IReducerRegistry
{
    private readonly Dictionary<Type, Delegate> _reducers = new();

    /// <summary>Register a reducer function for a specific action type.</summary>
    public void Register<TAction>(Func<AppState, TAction, AppState> reducer)
        where TAction : IAction
    {
        _reducers[typeof(TAction)] = reducer;
    }

    /// <summary>Reduce the current state by applying an action.</summary>
    public AppState Reduce(AppState state, IAction action)
    {
        var actionType = action.GetType();

        if (_reducers.TryGetValue(actionType, out var reducer))
        {
            var method = reducer.GetType().GetMethod("Invoke");
            if (method != null)
            {
                var result = method.Invoke(reducer, new object[] { state, action });
                if (result is AppState newState)
                {
                    return newState;
                }
            }
        }

        // If no reducer registered, return unchanged state
        return state;
    }
}
