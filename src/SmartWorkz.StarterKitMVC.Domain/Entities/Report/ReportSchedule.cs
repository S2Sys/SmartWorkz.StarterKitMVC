namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

public class ReportSchedule
{
    public int ReportScheduleId { get; set; }
    public int ReportId { get; set; }
    public string ScheduleName { get; set; }
    public string Frequency { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public bool IsActive { get; set; } = true;
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Report Report { get; set; }
}
