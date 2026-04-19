namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

using SmartWorkz.Core.Entities;

public class ReportData : AuditableEntity<long>
{
    public int ReportId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string DataJson { get; set; }
    public string Summary { get; set; }

    public Report Report { get; set; }
}
