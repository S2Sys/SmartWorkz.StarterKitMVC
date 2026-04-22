namespace SmartWorkz.Core;

/// <summary>
/// Enumeration representing the lifecycle status of an entity.
/// </summary>
/// <remarks>
/// Entity Lifecycle Framework:
/// The EntityStatus enum defines the state progression for domain entities throughout their lifetime.
/// This follows a soft-delete pattern where entities are marked for deletion rather than physically removed,
/// enabling data recovery and historical tracking.
///
/// Lifecycle Progression:
/// 1. Active (0) — Default state for newly created entities. Entity is visible in standard queries and available for business operations.
/// 2. Inactive (1) — Temporarily disabled state. Entity is preserved in the database but logically excluded from queries.
///    Used when an entity needs to be hidden but may be reactivated later (e.g., user account deactivation).
/// 3. Archived (2) — Historical state. Entity is retained for auditing and historical purposes but is not expected to be reactivated.
///    Typically used for time-based archival (e.g., completed projects, closed orders).
/// 4. Deleted (3) — Soft-deleted state. Entity is marked as deleted but remains in the database for data integrity and audit trails.
///    Physical deletion is typically not performed to preserve referential integrity and compliance logs.
///
/// Integration:
/// - Used in: Data access queries (WHERE Status = Active), entity filters, audit logging, business rule validation
/// - Default: Active (recommended initial value for all new entities)
/// - Transitions: Active → Inactive → Archived or Active → Inactive → Deleted
/// - Query Filters: Services should filter Active entities by default unless explicitly including other states
///
/// Example Scenario:
/// A customer entity starts as Active when registered. When they request account suspension, it becomes Inactive.
/// After 12 months of inactivity, it may transition to Archived. Permanent removal would set it to Deleted for compliance.
/// </remarks>
public enum EntityStatus
{
    /// <summary>
    /// Active state — Entity is fully operational and visible in standard queries.
    /// </summary>
    /// <remarks>
    /// When to use: Default state for all newly created entities. Use when entity should be fully available for business operations.
    ///
    /// Example Scenarios:
    /// - A newly registered user account
    /// - A freshly created product in inventory
    /// - An active project or order
    ///
    /// Query filtering: Standard queries should filter WHERE Status = Active unless explicitly including historical states.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Active")]
    Active = 0,

    /// <summary>
    /// Inactive state — Entity is temporarily disabled but preserved for potential reactivation.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity needs to be hidden from normal operations but may be needed again in the future.
    /// This is a reversible state with lower overhead than archival.
    ///
    /// Example Scenarios:
    /// - A user account temporarily suspended due to security concerns, but can be reactivated
    /// - A sales employee on leave who should not appear in assignments
    /// - A vendor temporarily unable to fulfill orders but expected to resume
    ///
    /// Characteristics:
    /// - Data is fully preserved and can be reactivated with minimal impact
    /// - Associated records (orders, relationships) typically remain linked but become invisible
    /// - Shorter duration expectation than Archived
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Inactive")]
    Inactive = 1,

    /// <summary>
    /// Archived state — Entity is retained for historical reference and audit purposes with no reactivation expected.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity's operational lifecycle is complete and its data should be preserved for historical,
    /// compliance, or statistical purposes without expectation of reactivation.
    ///
    /// Example Scenarios:
    /// - A completed project maintained for portfolio/audit purposes
    /// - A fulfilled order kept for customer history and financial records
    /// - A retired business unit preserved for historical analysis
    ///
    /// Characteristics:
    /// - Typically applies to entities with time-bound relevance
    /// - Data is immutable and preserved indefinitely
    /// - May be included in read-only queries for reporting/analytics
    /// - Generally not filtered from comprehensive audits
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Archived")]
    Archived = 2,

    /// <summary>
    /// Deleted state — Soft-deleted entity marked for removal but physically retained for data integrity.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity must be removed from normal operations due to user request, compliance requirements,
    /// or data retention policies, while maintaining referential integrity and audit trails.
    ///
    /// Example Scenarios:
    /// - A user account deleted per GDPR request (with anonymization of related records)
    /// - A transaction marked for deletion but retained in ledger for reconciliation
    /// - An organization removed from active use but kept for tax/compliance records
    ///
    /// Characteristics:
    /// - Data is preserved indefinitely per compliance and audit requirements
    /// - Excluded from all standard application queries
    /// - May be included only in administrative/forensic queries
    /// - Physical deletion is rare and requires explicit administrative action
    /// - Often paired with data masking/anonymization for privacy compliance
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Deleted")]
    Deleted = 3
}
