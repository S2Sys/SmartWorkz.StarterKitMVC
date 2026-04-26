using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Public.Pages;

/// <summary>
/// Base for all Public list pages backed by IDapperRepository&lt;T&gt;.
/// Wires search, sort, pagination, and HTMX partial response automatically.
/// </summary>
public abstract class BaseListPage<T> : BasePaging<T> where T : class, new()
{
    private readonly IDapperRepository<T> _repository;

    protected BaseListPage(IDapperRepository<T> repository)
    {
        _repository = repository;
    }

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
