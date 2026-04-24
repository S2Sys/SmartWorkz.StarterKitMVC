namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a new order is created.
/// </summary>
public class OrderCreatedEvent
{
    public OrderCreatedEvent(string orderId, string userId, decimal totalAmount, string status)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public string OrderId { get; }
    public string UserId { get; }
    public decimal TotalAmount { get; }
    public string Status { get; }
    public DateTime CreatedAt { get; }
}
