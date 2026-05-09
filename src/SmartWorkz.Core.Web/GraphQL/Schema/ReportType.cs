namespace SmartWorkz.Core.Web.GraphQL.Schema;

/// <summary>
/// GraphQL type for Report entity.
/// Exposes report information for analytics and reporting.
/// </summary>
public class ReportType
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public DateTime GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public string? Type { get; set; }

    public DateTime CreatedAt { get; set; }
}
