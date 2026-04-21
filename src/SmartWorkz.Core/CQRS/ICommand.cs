namespace SmartWorkz.Core;

/// <summary>
/// Marker interface for command objects representing intent to change state.
/// </summary>
/// <remarks>
/// Commands are part of the CQRS pattern and represent operations that modify state.
/// Each command should be handled by exactly one command handler.
///
/// This interface is re-exported from SmartWorkz.Shared for convenience.
/// </remarks>
public interface ICommand : SmartWorkz.Shared.ICommand
{
}
