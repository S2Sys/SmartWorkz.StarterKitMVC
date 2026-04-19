namespace SmartWorkz.Core.Shared.Base_Classes;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
