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
