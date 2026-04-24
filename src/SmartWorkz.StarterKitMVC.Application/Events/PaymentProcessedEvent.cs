namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a payment is successfully processed.
/// </summary>
public class PaymentProcessedEvent
{
    public PaymentProcessedEvent(string paymentId, string orderId, decimal amount, string status, string? transactionId = null)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Status = status;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    public string PaymentId { get; }
    public string OrderId { get; }
    public decimal Amount { get; }
    public string Status { get; }
    public string? TransactionId { get; }
    public DateTime ProcessedAt { get; }
}
