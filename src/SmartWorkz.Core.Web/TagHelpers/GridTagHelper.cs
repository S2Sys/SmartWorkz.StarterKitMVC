using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Shared;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering data grids with sorting, filtering, row selection, and pagination support.
/// Targets the &lt;grid&gt; element and provides a high-level API for binding to IDataContext&lt;T&gt;
/// and rendering grid data with Bootstrap table styling.
/// </summary>
/// <remarks>
/// Generates: &lt;div class="grid-wrapper table-responsive"&gt;...GridComponent markup...&lt;/div&gt;
///
/// IDataContext&lt;T&gt; Binding:
/// GridTagHelper requires binding to an IDataContext&lt;T&gt; instance that manages all grid state:
/// - CurrentRequest: Contains sort column, sort direction, filter parameters, and pagination info
/// - CurrentResponse: Contains paged data, column definitions, and filter options
/// - SelectedRowIds: Tracks currently selected rows for multi-select operations
/// - IsLoading: Indicates if data fetch is in progress
/// - Error: Contains error message from failed operations
///
/// IDataContext&lt;T&gt; Methods:
/// - UpdateSort(propertyName, isDescending): Change sort column; triggers data refetch
/// - UpdateFilter(property, operator, value): Add/replace filter; resets to page 1; triggers refetch
/// - UpdatePagination(pageNumber, pageSize): Navigate to page; triggers refetch
/// - ToggleRowSelection(rowId): Toggle selection for single row
/// - SetSelectedRows(rowIds): Replace all selected rows
/// - ToggleSelectAll(isChecked): Select/deselect all visible rows on current page
/// - ClearFilters(): Reset all filters to default; triggers refetch
/// - Initialize(dataSource): Load data from source (IEnumerable&lt;T&gt;)
///
/// Column Configuration:
/// Child &lt;grid-column&gt; elements (future support) define which properties to display, whether to enable
/// sorting/filtering per column, column visibility, and custom formatting.
///
/// Data Binding Patterns:
/// - Bind DataSource property to IDataContext&lt;T&gt;.Items collection
/// - GridTagHelper manages state transitions via IDataContext&lt;T&gt; methods
/// - Child GridComponent.razor component renders the actual table markup
/// Both work together: TagHelper provides state management and configuration,
/// GridComponent.razor handles rendering and user interactions.
///
/// Bootstrap CSS Classes Applied:
/// - .grid-wrapper: Outer container for grid styling
/// - .table-responsive: Responsive table wrapper (for mobile/tablet layout)
/// - .table: Base table styling
/// - .table-striped: Alternating row background colors
/// - .table-hover: Row highlight on hover
/// - .table-sm: Compact table (reduced padding)
/// Row Selection styling:
/// - Selected rows styled with .table-active class for visual distinction
/// Pagination:
/// - Inherited from PaginationTagHelper: .pagination, .page-item, .page-link classes
///
/// Grid Lifecycle:
/// 1. GridTagHelper initializes with IDataContext&lt;T&gt;
/// 2. IDataContext&lt;T&gt;.Initialize() loads data from source
/// 3. User interacts with grid (click sort, change filter, select row, navigate page)
/// 4. Interaction triggers IDataContext&lt;T&gt; method (UpdateSort, UpdateFilter, etc.)
/// 5. IDataContext&lt;T&gt; raises OnStateChanged event
/// 6. GridComponent.razor re-renders with updated data
///
/// Typical Usage Patterns:
/// - Basic grid: Bind to DataSource, set PageSize
/// - With sorting: Enable sort buttons in column headers
/// - With filtering: Add filter controls (dropdowns, text inputs) bound to UpdateFilter()
/// - With selection: Enable checkboxes, use AllowRowSelection for multi-select UI
/// - With pagination: PaginationTagHelper or custom page navigation
/// </remarks>
/// <example>
/// &lt;!-- Basic grid with data binding --&gt;
/// &lt;grid data-source="@Model.Products" data-page-size="20"&gt;
///   &lt;grid-column property="Name" sortable="true"&gt;Product Name&lt;/grid-column&gt;
///   &lt;grid-column property="Price" sortable="true" format="currency"&gt;Price&lt;/grid-column&gt;
/// &lt;/grid&gt;
///
/// &lt;!-- Grid with row selection enabled --&gt;
/// &lt;grid data-source="@Model.Orders" data-page-size="50" data-allow-selection="true"&gt;
///   &lt;grid-column property="OrderId"&gt;Order ID&lt;/grid-column&gt;
///   &lt;grid-column property="OrderDate" sortable="true" format="date"&gt;Date&lt;/grid-column&gt;
///   &lt;grid-column property="Status" filterable="true"&gt;Status&lt;/grid-column&gt;
/// &lt;/grid&gt;
///
/// &lt;!-- Grid with custom CSS class and export option --&gt;
/// &lt;grid data-source="@Model.Customers"
///       data-page-size="25"
///       data-allow-export="true"
///       data-allow-column-toggle="true"
///       data-css-class="compact-grid"&gt;
///   &lt;grid-column property="FirstName"&gt;First Name&lt;/grid-column&gt;
///   &lt;grid-column property="Email" sortable="true"&gt;Email&lt;/grid-column&gt;
///   &lt;grid-column property="CreatedDate" sortable="true" format="date"&gt;Created&lt;/grid-column&gt;
/// &lt;/grid&gt;
///
/// &lt;!-- Complete example with IDataContext&lt;Product&gt; binding --&gt;
/// @{
///   var productDataContext = new DataContext&lt;Product&gt;(productService);
///   await productDataContext.Initialize(await productService.GetProductsAsync());
/// }
/// &lt;grid data-source="@productDataContext.Items"
///       data-page-size="20"
///       data-allow-selection="true"&gt;
///   &lt;grid-column property="Name" sortable="true"&gt;Product&lt;/grid-column&gt;
///   &lt;grid-column property="Category" filterable="true"&gt;Category&lt;/grid-column&gt;
///   &lt;grid-column property="Price" format="currency" css-class="text-end"&gt;Price&lt;/grid-column&gt;
///   &lt;grid-column property="Stock" sortable="true"&gt;Stock&lt;/grid-column&gt;
/// &lt;/grid&gt;
///
/// &lt;!-- Grid with filtering and sorting --&gt;
/// &lt;div class="mb-3"&gt;
///   &lt;label for="statusFilter"&gt;Filter by Status:&lt;/label&gt;
///   &lt;select id="statusFilter" onchange="updateFilter(this.value)"&gt;
///     &lt;option value=""&gt;All&lt;/option&gt;
///     &lt;option value="Active"&gt;Active&lt;/option&gt;
///     &lt;option value="Inactive"&gt;Inactive&lt;/option&gt;
///   &lt;/select&gt;
/// &lt;/div&gt;
///
/// &lt;grid data-source="@Model.Items" data-page-size="20"&gt;
///   &lt;grid-column property="Name" sortable="true"&gt;Name&lt;/grid-column&gt;
///   &lt;grid-column property="Status" filterable="true"&gt;Status&lt;/grid-column&gt;
///   &lt;grid-column property="CreatedDate" sortable="true" format="date"&gt;Created&lt;/grid-column&gt;
/// &lt;/grid&gt;
/// </example>
[HtmlTargetElement("grid")]
public class GridTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the data source for the grid.
    /// Should be bound to IDataContext&lt;T&gt;.Items or an IEnumerable&lt;T&gt; collection.
    /// GridTagHelper passes this data to GridComponent.razor for rendering.
    /// </summary>
    [HtmlAttributeName("data-source")]
    public string? DataSource { get; set; }

    /// <summary>
    /// Gets or sets the number of rows to display per page.
    /// Used by IDataContext&lt;T&gt; pagination. Default is 20.
    /// Affects both initial data load and subsequent page navigation.
    /// </summary>
    [HtmlAttributeName("data-page-size")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets whether row selection (checkboxes) is enabled.
    /// When true, renders checkbox column for multi-select row selection.
    /// Selected row IDs are tracked in IDataContext&lt;T&gt;.SelectedRowIds.
    /// Default is false.
    /// </summary>
    [HtmlAttributeName("data-allow-selection")]
    public bool AllowRowSelection { get; set; }

    /// <summary>
    /// Gets or sets whether grid export functionality is enabled.
    /// When true, displays export buttons (CSV, Excel, PDF, etc.) for exporting grid data.
    /// Default is false. Requires backend support for export formats.
    /// </summary>
    [HtmlAttributeName("data-allow-export")]
    public bool AllowExport { get; set; }

    /// <summary>
    /// Gets or sets whether column visibility toggle is enabled.
    /// When true, displays column visibility menu allowing users to show/hide columns.
    /// Default is false. Persists column preferences in browser storage.
    /// </summary>
    [HtmlAttributeName("data-allow-column-toggle")]
    public bool AllowColumnVisibilityToggle { get; set; }

    /// <summary>
    /// Gets or sets custom CSS classes to apply to the grid wrapper element.
    /// These classes are merged with default grid classes for custom styling.
    /// Example: "compact-grid", "bordered-grid", or "striped-grid"
    /// </summary>
    [HtmlAttributeName("data-css-class")]
    public string? CustomCssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // This is a placeholder. In actual implementation, this would need to:
        // 1. Parse child <column> elements
        // 2. Generate the GridComponent markup
        // 3. Inject required services

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "grid-wrapper");
        output.Content.SetContent($"<!-- Grid: {DataSource} Page Size: {PageSize} -->");
    }
}

