namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

public class ReportData
{
    public long ReportDataId { get; set; }
    public int ReportId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string DataJson { get; set; }
    public string Summary { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Report Report { get; set; }
}
