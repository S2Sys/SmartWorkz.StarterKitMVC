using SmartWorkz.Core.Wiki.Models;

namespace SmartWorkz.Core.Wiki.Services;

public class WikiDocumentService
{
    private readonly string _docsRoot;
    private List<WikiCategory>? _cachedCategories;

    public WikiDocumentService(IWebHostEnvironment env)
    {
        _docsRoot = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "docs"));
    }

    public List<WikiCategory> GetCategories()
    {
        if (_cachedCategories != null)
            return _cachedCategories;

        var categories = new List<WikiCategory>();

        if (!Directory.Exists(_docsRoot))
            return categories;

        // Root-level markdown files
        var rootFiles = Directory.GetFiles(_docsRoot, "*.md", SearchOption.TopDirectoryOnly)
            .Select(f => new WikiDocument
            {
                Title = FormatTitle(Path.GetFileNameWithoutExtension(f)),
                Slug = "root-" + Path.GetFileNameWithoutExtension(f).ToLower().Replace("_", "-").Replace(".", "-"),
                Category = "Core Guides",
                RelativePath = Path.GetFileName(f)
            }).ToList();

        if (rootFiles.Count > 0)
            categories.Add(new WikiCategory { Name = "Core Guides", Slug = "core-guides", Documents = rootFiles });

        // Subdirectory-based categories
        foreach (var dir in Directory.GetDirectories(_docsRoot).Where(d => !Path.GetFileName(d).StartsWith(".")))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName == "superpowers") continue;

            var files = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly)
                .Select(f => new WikiDocument
                {
                    Title = FormatTitle(Path.GetFileNameWithoutExtension(f)),
                    Slug = dirName + "-" + Path.GetFileNameWithoutExtension(f).ToLower().Replace("_", "-").Replace(".", "-"),
                    Category = FormatTitle(dirName),
                    RelativePath = dirName + "/" + Path.GetFileName(f)
                }).ToList();

            if (files.Count > 0)
                categories.Add(new WikiCategory { Name = FormatTitle(dirName), Slug = dirName, Documents = files });
        }

        _cachedCategories = categories;
        return categories;
    }

    public WikiDocument? GetDocument(string slug)
    {
        var all = GetCategories().SelectMany(c => c.Documents);
        return all.FirstOrDefault(d => d.Slug == slug);
    }

    public string? RenderDocument(string slug)
    {
        var doc = GetDocument(slug);
        if (doc == null) return null;

        var filePath = Path.Combine(_docsRoot, doc.RelativePath);
        if (!File.Exists(filePath)) return null;

        var markdown = File.ReadAllText(filePath);
        return Markdig.Markdown.ToHtml(markdown);
    }

    private static string FormatTitle(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        var parts = name.Split(['-', '_']);
        return string.Join(" ", parts.Select(w => char.ToUpper(w[0]) + w[1..]));
    }
}
