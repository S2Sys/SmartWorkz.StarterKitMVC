using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Admin.Pages;

/// <summary>
/// Base for all Admin list pages backed by IDapperRepository&lt;T&gt;.
/// Wires search, sort, pagination, and HTMX partial response automatically.
///
/// Usage:
///   public class IndexModel : BaseListPage&lt;User&gt;
///   {
///       public IndexModel(IDapperRepository&lt;User&gt; repo) : base(repo) { }
///       // Override BuildFilter() to add entity-specific WHERE conditions
///   }
/// </summary>
public abstract class BaseListPage<T> : BasePage where T : class, new()
{
    private readonly IDapperRepository<T> _repository;

    protected BaseListPage(IDapperRepository<T> repository)
    {
        _repository = repository;
    }

    // ── Bound query params ────────────────────────────────────────────────────

    [BindProperty(SupportsGet = true)] public string? Search    { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page      { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int     PageSize  { get; set; } = 20;
    [BindProperty(SupportsGet = true)] public string  SortBy    { get; set; } = "CreatedAt";
    [BindProperty(SupportsGet = true)] public bool    Desc      { get; set; } = true;

    // ── Result ────────────────────────────────────────────────────────────────

    public IEnumerable<T>  Items      { get; private set; } = [];
    public int             Total      { get; private set; }
    public int             PageCount  => (int)Math.Ceiling((double)Total / PageSize);
    public PaginationModel Pagination { get; private set; } = PaginationModel.From(0, 1, 20);

    // ── Load ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call from OnGetAsync / OnGetTableAsync.
    /// Pass htmxTarget/htmxHandler when list page uses HTMX partial updates.
    /// </summary>
    protected async Task LoadAsync(string? htmxTarget = null, string? htmxHandler = null)
    {
        var filter = BuildFilter();
        (Items, Total) = await _repository.GetPagedAsync(filter, SortBy, Desc, Page, PageSize);

        var routeValues = new Dictionary<string, string?>
        {
            ["search"] = Search,
            ["sortBy"] = SortBy,
            ["desc"]   = Desc.ToString().ToLower(),
        };

        Pagination = PaginationModel.From(Total, Page, PageSize, routeValues, htmxTarget, htmxHandler);
    }

    /// <summary>
    /// Override to supply entity-specific filter object.
    /// Default uses TenantId only.
    /// </summary>
    protected virtual object BuildFilter() => new { TenantId };

    // ── HTMX partial helper ───────────────────────────────────────────────────

    /// <summary>
    /// Returns a partial for HTMX requests, full page otherwise.
    /// Call this instead of Page() in OnGetAsync.
    /// </summary>
    protected IActionResult PageOrPartial(string partialName)
        => Request.IsHtmx() ? Partial(partialName, this) : Page();
}
