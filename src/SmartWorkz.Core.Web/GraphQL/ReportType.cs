using HotChocolate.Types;

namespace SmartWorkz.Core.Web.GraphQL;

/// <summary>
/// GraphQL type representing a report in the system.
/// </summary>
[ObjectType]
public class ReportType
{
    /// <summary>
    /// Unique identifier for the report.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Report title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Report content/body.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the report was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// User ID of the person who generated the report.
    /// </summary>
    public string GeneratedBy { get; set; } = string.Empty;

    /// <summary>
    /// Type of report (e.g., Sales, Analytics, User Activity).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// When the report record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the report record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
