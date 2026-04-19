namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

public class EmailQueue
{
    public int EmailQueueId { get; set; }
    public string ToEmail { get; set; }
    public string CcEmail { get; set; }
    public string BccEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public string Status { get; set; } = "Pending";
    public int SendAttempts { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string FailureReason { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
