namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a payment is successfully completed.
/// </summary>
public class PaymentCompletedEvent
{
    public PaymentCompletedEvent(string paymentId, string orderId, string? transactionId = null)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
    }

    public string PaymentId { get; }
    public string OrderId { get; }
    public string? TransactionId { get; }
    public DateTime CompletedAt { get; }
}
