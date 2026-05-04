namespace SmartWorkz.Mobile.State.Actions;

/// <summary>Base interface for all Redux actions.</summary>
public interface IAction
{
    /// <summary>Gets the action type identifier.</summary>
    string Type { get; }
}
