namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when an order is processed.
/// </summary>
public class OrderProcessedEvent
{
    public OrderProcessedEvent(string orderId, string userId, decimal amount)
    {
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        ProcessedAt = DateTime.UtcNow;
    }

    public string OrderId { get; }
    public string UserId { get; }
    public decimal Amount { get; }
    public DateTime ProcessedAt { get; }
}
