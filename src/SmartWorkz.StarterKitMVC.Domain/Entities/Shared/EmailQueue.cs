namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class EmailQueue : AuditableEntity<int>
{
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
}
