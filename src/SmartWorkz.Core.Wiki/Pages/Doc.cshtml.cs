using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.Core.Wiki.Models;
using SmartWorkz.Core.Wiki.Services;

namespace SmartWorkz.Core.Wiki.Pages;

public class DocModel : PageModel
{
    private readonly WikiDocumentService _wikiService;

    public WikiDocument? Document { get; set; }
    public string? HtmlContent { get; set; }
    public WikiDocument? PreviousDocument { get; set; }
    public WikiDocument? NextDocument { get; set; }

    public DocModel(WikiDocumentService wikiService)
    {
        _wikiService = wikiService;
    }

    public void OnGet(string? slug)
    {
        if (string.IsNullOrEmpty(slug))
            return;

        Document = _wikiService.GetDocument(slug);
        if (Document == null)
            return;

        HtmlContent = _wikiService.RenderDocument(slug);

        // Get previous/next for navigation
        var allDocs = _wikiService.GetCategories()
            .SelectMany(c => c.Documents)
            .ToList();

        var currentIndex = allDocs.FindIndex(d => d.Slug == slug);
        if (currentIndex > 0)
            PreviousDocument = allDocs[currentIndex - 1];
        if (currentIndex < allDocs.Count - 1)
            NextDocument = allDocs[currentIndex + 1];
    }
}
