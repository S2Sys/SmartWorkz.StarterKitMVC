namespace SmartWorkz.Core.CQRS;

using SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Marker interface for command objects representing intent to change state.
/// </summary>
/// <remarks>
/// Commands are part of the CQRS pattern and represent operations that modify state.
/// Each command should be handled by exactly one command handler.
///
/// This interface is re-exported from SmartWorkz.Core.Shared for convenience.
/// </remarks>
public interface ICommand : SmartWorkz.Core.Shared.CQRS.ICommand
{
}
