namespace SmartWorkz.StarterKitMVC.Domain.Entities.Transaction;

using SmartWorkz.Core.Entities;

public class TransactionLog : AuditableEntity<long>
{
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
}
