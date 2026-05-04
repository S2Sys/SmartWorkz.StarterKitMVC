namespace SmartWorkz.Mobile.State.Actions;

/// <summary>Base interface for all Redux actions.</summary>
public interface IAction
{
    /// <summary>Unique action type identifier. Each action must explicitly define.</summary>
    string Type { get; }
}
