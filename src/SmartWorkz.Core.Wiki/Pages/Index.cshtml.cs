using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.Core.Wiki.Models;
using SmartWorkz.Core.Wiki.Services;

namespace SmartWorkz.Core.Wiki.Pages;

public class IndexModel : PageModel
{
    private readonly WikiDocumentService _wikiService;

    public List<WikiCategory> Categories { get; set; } = new();

    public IndexModel(WikiDocumentService wikiService)
    {
        _wikiService = wikiService;
    }

    public void OnGet()
    {
        Categories = _wikiService.GetCategories();
    }
}
