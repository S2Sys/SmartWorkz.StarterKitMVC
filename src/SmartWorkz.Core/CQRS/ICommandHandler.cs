namespace SmartWorkz.Core;

using SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Handler for processing a specific command type.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, must implement <see cref="ICommand"/>.</typeparam>
/// <remarks>
/// Command handlers encapsulate the logic for handling state-changing operations.
/// Each command type should have exactly one handler, but handlers can be registered
/// multiple times in the dependency injection container if needed for different scenarios.
///
/// This interface is re-exported from SmartWorkz.Core.Shared for convenience.
/// </remarks>
public interface ICommandHandler<in TCommand> : SmartWorkz.Core.Shared.CQRS.ICommandHandler<TCommand> where TCommand : ICommand
{
}
