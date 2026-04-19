namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

using SmartWorkz.Core.Entities;

public class Report : AuditableEntity<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ReportType { get; set; }
    public string QueryDefinition { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ReportSchedule> ReportSchedules { get; set; }
    public ICollection<ReportData> ReportDataCollection { get; set; }
}
