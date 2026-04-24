namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when an order is shipped.
/// </summary>
public class OrderShippedEvent
{
    public OrderShippedEvent(string orderId, string trackingNumber, string carrier)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
        ShippedAt = DateTime.UtcNow;
    }

    public string OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }
    public DateTime ShippedAt { get; }
}
