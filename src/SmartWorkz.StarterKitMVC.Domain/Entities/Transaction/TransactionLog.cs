namespace SmartWorkz.StarterKitMVC.Domain.Entities.Transaction;

public class TransactionLog
{
    public long TransactionLogId { get; set; }
    public string TransactionType { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public decimal Amount { get; set; }
    public int? CurrencyId { get; set; }
    public string Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentMethod { get; set; }
    public string ReferenceNumber { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string FailureReason { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
