using HotChocolate.Types;

namespace SmartWorkz.Core.Web.GraphQL;

/// <summary>
/// GraphQL type representing a transaction in the system.
/// </summary>
[ObjectType]
public class TransactionType
{
    /// <summary>
    /// Unique identifier for the transaction.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction status (e.g., Completed, Pending, Failed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// User ID associated with this transaction.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the transaction was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
