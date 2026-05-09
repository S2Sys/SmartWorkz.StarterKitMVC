namespace SmartWorkz.Core.Web.GraphQL.Schema;

/// <summary>
/// GraphQL type for Transaction entity.
/// Exposes transaction information for financial reporting.
/// </summary>
public class TransactionType
{
    public string Id { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
