using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Shared.Pages;

/// <summary>
/// Generic paging base extending BasePage<T> with pagination state.
/// Carries bound query params (page, size, search, sort) and result properties (items, total, pagination).
/// LoadAsync is NOT here — it stays in concrete BaseListPage<T> per portal (Admin/Public).
/// </summary>
public abstract class BasePaging<T> : BasePage<T> where T : class
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "CreatedAt";

    [BindProperty(SupportsGet = true)]
    public bool Desc { get; set; } = true;

    public IEnumerable<T> Items { get; protected set; } = [];
    public int Total { get; protected set; }
    public int PageCount => (int)Math.Ceiling((double)Total / PageSize);
    public PaginationModel Pagination { get; protected set; } = PaginationModel.From(0, 1, 20);
}
