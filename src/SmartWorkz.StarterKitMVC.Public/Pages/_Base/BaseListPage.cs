using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Public.Pages;

/// <summary>
/// Base for all Public list pages backed by IDapperRepository&lt;T&gt;.
/// Wires search, sort, pagination, and HTMX partial response automatically.
/// </summary>
public abstract class BaseListPage<T> : BasePage where T : class, new()
{
    private readonly IDapperRepository<T> _repository;

    protected BaseListPage(IDapperRepository<T> repository)
    {
        _repository = repository;
    }

    [BindProperty(SupportsGet = true)] public string? Search   { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page     { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int     PageSize { get; set; } = 20;
    [BindProperty(SupportsGet = true)] public string  SortBy   { get; set; } = "CreatedAt";
    [BindProperty(SupportsGet = true)] public bool    Desc     { get; set; } = true;

    public IEnumerable<T>  Items      { get; private set; } = [];
    public int             Total      { get; private set; }
    public int             PageCount  => (int)Math.Ceiling((double)Total / PageSize);
    public PaginationModel Pagination { get; private set; } = PaginationModel.From(0, 1, 20);

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

    protected virtual object BuildFilter() => new { TenantId };

    protected IActionResult PageOrPartial(string partialName)
        => Request.IsHtmx() ? Partial(partialName, this) : Page();
}
