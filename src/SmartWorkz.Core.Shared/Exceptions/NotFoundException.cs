namespace SmartWorkz.Core.Shared.Exceptions;

public class NotFoundException : ApplicationException
{
    public string? EntityName { get; }
    public object? EntityId { get; }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
