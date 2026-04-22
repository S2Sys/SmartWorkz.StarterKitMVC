namespace SmartWorkz.Core;

/// <summary>
/// Enumeration representing the sort order direction for query results.
/// </summary>
/// <remarks>
/// Sort Direction Framework:
/// SortDirection specifies whether query results should be ordered in ascending or descending sequence.
/// It is used in conjunction with a sort field to determine result ordering for UI display and reporting.
///
/// Integration:
/// - Used in: Data queries, search filters, pagination, API sort parameters
/// - Default: Ascending (recommended for alphabetical, chronological, or numerical sequences)
/// - Mapping: Ascending → ORDER BY field ASC, Descending → ORDER BY field DESC
/// - UI Integration: Sort indicators and sort toggles (typically display arrow up/down)
/// - Query Building: Combined with a FieldName parameter to form complete sort specification
///
/// Example Scenario:
/// A customer list can be sorted by Name in ascending (A→Z) or descending (Z→A) order.
/// Date-based sorts might show newest-first (Descending) or oldest-first (Ascending).
///
/// Common Usage Patterns:
/// - Alphabetical: Names, product titles use Ascending by default
/// - Chronological: Dates use Descending for "newest first" by default
/// - Numerical: Prices, quantities use context-dependent defaults
/// - UI Click Toggle: User clicks column header to toggle between Ascending and Descending
/// </remarks>
public enum SortDirection
{
    /// <summary>
    /// Ascending sort order — Results ordered from smallest to largest, A to Z, earliest to latest.
    /// </summary>
    /// <remarks>
    /// When to use: For alphabetical sequences, ascending numerical order, or chronological order (earliest first).
    /// This is typically the default for most human-readable lists.
    ///
    /// Example Scenarios:
    /// - Customer names (A-Z): Alice, Bob, Charlie
    /// - Product prices (low to high): $5.00, $10.00, $25.00
    /// - Event dates (oldest first): Jan 2024, Feb 2024, Mar 2024
    /// - Age (youngest first): 18, 25, 40
    ///
    /// SQL Mapping: ORDER BY FieldName ASC
    /// UI Indicator: Upward-pointing arrow
    ///
    /// Database Performance: No special performance consideration; indexes support both directions.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Ascending")]
    Ascending = 0,

    /// <summary>
    /// Descending sort order — Results ordered from largest to smallest, Z to A, latest to earliest.
    /// </summary>
    /// <remarks>
    /// When to use: For reverse alphabetical order, descending numerical order, or reverse chronological order (newest first).
    /// Often preferred for time-based data where users want recent items visible first.
    ///
    /// Example Scenarios:
    /// - Customer names (Z-A): Charlie, Bob, Alice
    /// - Transaction amounts (high to low): $1000, $500, $100
    /// - Event dates (newest first): Mar 2024, Feb 2024, Jan 2024
    /// - Age (oldest first): 65, 45, 25
    /// - Search results (most relevant first): Highest score → Lowest score
    ///
    /// SQL Mapping: ORDER BY FieldName DESC
    /// UI Indicator: Downward-pointing arrow
    ///
    /// Common Default: Often used as default for time-series data (activity feeds, notifications, transaction histories)
    /// to show most recent items first.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Descending")]
    Descending = 1
}
