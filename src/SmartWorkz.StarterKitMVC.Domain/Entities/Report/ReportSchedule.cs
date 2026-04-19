namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

using SmartWorkz.Core.Entities;

public class ReportSchedule : AuditableEntity<int>
{
    public int ReportId { get; set; }
    public string ScheduleName { get; set; }
    public string Frequency { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public bool IsActive { get; set; } = true;

    public Report Report { get; set; }
}
