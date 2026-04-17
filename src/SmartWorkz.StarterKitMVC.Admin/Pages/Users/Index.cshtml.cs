using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Users;

// [Authorize(Policy = "RequireAdmin")]
public class IndexModel : BasePage
{
    private readonly IUserRepository _userRepository;

    public IndexModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [BindProperty(SupportsGet = true)] public string? Search   { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page     { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int     PageSize { get; set; } = 20;
    [BindProperty(SupportsGet = true)] public string  SortBy   { get; set; } = "CreatedAt";
    [BindProperty(SupportsGet = true)] public bool    Desc     { get; set; } = true;

    public IEnumerable<User>  Users      { get; private set; } = [];
    public PaginationModel    Pagination { get; private set; } = PaginationModel.From(0, 1, 20);

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnGetTableAsync()
    {
        await LoadUsersAsync();
        return Request.IsHtmx() ? Partial("_UserTableRows", this) : Page();
    }

    private async Task LoadUsersAsync()
    {
        var (items, total) = await _userRepository.SearchPagedAsync(
            TenantId, Search, SortBy, Desc, Page, PageSize);

        Users = items;

        Pagination = PaginationModel.From(total, Page, PageSize,
            routeValues: new Dictionary<string, string?>
            {
                ["search"] = Search,
                ["sortBy"] = SortBy,
                ["desc"]   = Desc.ToString().ToLower(),
            },
            htmxTarget: "#users-table-container");
    }
}
