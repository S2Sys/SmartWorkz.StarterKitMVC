using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.Core.Wiki.Services;

namespace SmartWorkz.Core.Wiki.Pages.Api;

public class DocsModel : PageModel
{
    private readonly WikiDocumentService _wikiService;

    public DocsModel(WikiDocumentService wikiService)
    {
        _wikiService = wikiService;
    }

    public IActionResult OnGet()
    {
        var docs = _wikiService.GetCategories()
            .SelectMany(c => c.Documents.Select(d => new
            {
                d.Slug,
                d.Title,
                d.Category
            }))
            .ToList();

        return new JsonResult(docs);
    }
}
