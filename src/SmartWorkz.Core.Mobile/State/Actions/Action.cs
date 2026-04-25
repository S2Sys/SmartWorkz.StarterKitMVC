namespace SmartWorkz.Mobile.State.Actions;

/// <summary>Base interface for all Redux actions.</summary>
public interface IAction
{
    /// <summary>Unique action type identifier.</summary>
    string Type => GetType().Name;
}
