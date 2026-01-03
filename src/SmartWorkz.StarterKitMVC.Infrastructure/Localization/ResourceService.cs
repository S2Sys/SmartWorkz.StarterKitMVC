using System.Text.Json;
using System.Xml.Linq;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Domain.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Localization;

/// <summary>
/// JSON file-based implementation of resource service
/// </summary>
public class ResourceService : IResourceService
{
    private readonly string _dataPath;
    private readonly object _lock = new();
    
    private List<Language> _languages = [];
    private List<Resource> _resources = [];
    private List<ResourceTranslation> _translations = [];
    private bool _isLoaded;

    public ResourceService(string? storagePath = null)
    {
        _dataPath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartWorkz", "StarterKitMVC", "localization");
        
        Directory.CreateDirectory(_dataPath);
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        
        lock (_lock)
        {
            if (_isLoaded) return;
            
            var languagesPath = Path.Combine(_dataPath, "languages.json");
            var resourcesPath = Path.Combine(_dataPath, "resources.json");
            var translationsPath = Path.Combine(_dataPath, "translations.json");
            
            if (File.Exists(languagesPath))
                _languages = JsonSerializer.Deserialize<List<Language>>(File.ReadAllText(languagesPath)) ?? [];
            
            if (File.Exists(resourcesPath))
                _resources = JsonSerializer.Deserialize<List<Resource>>(File.ReadAllText(resourcesPath)) ?? [];
            
            if (File.Exists(translationsPath))
                _translations = JsonSerializer.Deserialize<List<ResourceTranslation>>(File.ReadAllText(translationsPath)) ?? [];
            
            if (_languages.Count == 0)
                SeedDefaults();
            
            _isLoaded = true;
        }
        
        await Task.CompletedTask;
    }

    private void SeedDefaults()
    {
        _languages =
        [
            new() { Code = "en", Name = "English", NativeName = "English", IsDefault = true, SortOrder = 1 },
            new() { Code = "es", Name = "Spanish", NativeName = "Español", SortOrder = 2 },
            new() { Code = "fr", Name = "French", NativeName = "Français", SortOrder = 3 },
            new() { Code = "de", Name = "German", NativeName = "Deutsch", SortOrder = 4 },
            new() { Code = "ar", Name = "Arabic", NativeName = "العربية", IsRtl = true, SortOrder = 5 },
        ];

        // Create flat resource structure using dot notation keys
        _resources = [];
        
        // Add all resources using the ResourceKeys pattern
        var resourceDefinitions = GetDefaultResourceDefinitions();
        foreach (var (key, category, module) in resourceDefinitions)
        {
            _resources.Add(new Resource 
            { 
                Key = key, 
                Category = category, 
                Module = module, 
                IsSystem = true, 
                SortOrder = _resources.Count + 1 
            });
        }

        // Add English translations
        var enTranslations = GetDefaultEnglishTranslations();
        foreach (var resource in _resources)
        {
            if (enTranslations.TryGetValue(resource.Key, out var value))
            {
                _translations.Add(new ResourceTranslation
                {
                    ResourceId = resource.Id,
                    LanguageCode = "en",
                    Value = value,
                    Status = TranslationStatus.Published
                });
            }
        }

        SaveAll();
    }
    
    /// <summary>
    /// Synchronizes default resources from the embedded .resx file with existing data.
    /// Adds any new resources that don't exist yet without overwriting existing ones.
    /// Call this on application startup to ensure all resources are available.
    /// </summary>
    public async Task SyncDefaultResourcesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        // Read resources from the .resx file
        var resxResources = ReadDefaultResourcesFromResx();
        var existingKeys = _resources.Select(r => r.Key.ToLowerInvariant()).ToHashSet();
        var hasChanges = false;
        
        foreach (var (key, value, category, module) in resxResources)
        {
            if (existingKeys.Contains(key.ToLowerInvariant()))
                continue;
                
            // Add new resource
            var resource = new Resource 
            { 
                Key = key, 
                Category = category, 
                Module = module, 
                IsSystem = true, 
                SortOrder = _resources.Count + 1 
            };
            _resources.Add(resource);
            
            // Add English translation
            _translations.Add(new ResourceTranslation
            {
                ResourceId = resource.Id,
                LanguageCode = "en",
                Value = value,
                Status = TranslationStatus.Published
            });
            
            hasChanges = true;
        }
        
        if (hasChanges)
        {
            SaveAll();
        }
    }
    
    /// <summary>
    /// Reads default resources from the DefaultResources.resx file.
    /// Returns a list of (Key, Value, Category, Module) tuples.
    /// </summary>
    private static List<(string Key, string Value, string Category, string Module)> ReadDefaultResourcesFromResx()
    {
        var result = new List<(string Key, string Value, string Category, string Module)>();
        
        // Try multiple locations to find the resx file
        var baseDir = AppContext.BaseDirectory;
        var possiblePaths = new[]
        {
            // Output directory structure (after build)
            Path.Combine(baseDir, "Localization", "Resources", "DefaultResources.resx"),
            // Direct in output
            Path.Combine(baseDir, "Resources", "DefaultResources.resx"),
            // Flat structure
            Path.Combine(baseDir, "DefaultResources.resx"),
        };
        
        var resxPath = possiblePaths.FirstOrDefault(File.Exists);
        
        if (resxPath == null)
        {
            // Return empty if file not found - will use hardcoded defaults as fallback
            return result;
        }
        
        try
        {
            var doc = XDocument.Load(resxPath);
            var dataElements = doc.Descendants("data");
            
            foreach (var data in dataElements)
            {
                var key = data.Attribute("name")?.Value;
                var value = data.Element("value")?.Value;
                var comment = data.Element("comment")?.Value ?? "";
                
                if (string.IsNullOrEmpty(key) || value == null)
                    continue;
                
                // Parse category and module from comment (format: "Category=X;Module=Y")
                var category = "Labels";
                var module = "Common";
                
                if (!string.IsNullOrEmpty(comment))
                {
                    var parts = comment.Split(';');
                    foreach (var part in parts)
                    {
                        var kv = part.Split('=');
                        if (kv.Length == 2)
                        {
                            if (kv[0].Trim().Equals("Category", StringComparison.OrdinalIgnoreCase))
                                category = kv[1].Trim();
                            else if (kv[0].Trim().Equals("Module", StringComparison.OrdinalIgnoreCase))
                                module = kv[1].Trim();
                        }
                    }
                }
                
                result.Add((key, value, category, module));
            }
        }
        catch
        {
            // If parsing fails, return empty list - will use hardcoded defaults
        }
        
        return result;
    }
    
    private static List<(string Key, string Category, string Module)> GetDefaultResourceDefinitions()
    {
        return
        [
            // Common Labels - Basic
            ("common.labels.name", "Labels", "Common"),
            ("common.labels.title", "Labels", "Common"),
            ("common.labels.description", "Labels", "Common"),
            ("common.labels.email", "Labels", "Common"),
            ("common.labels.phone", "Labels", "Common"),
            ("common.labels.mobile", "Labels", "Common"),
            ("common.labels.fax", "Labels", "Common"),
            ("common.labels.website", "Labels", "Common"),
            ("common.labels.url", "Labels", "Common"),
            ("common.labels.address", "Labels", "Common"),
            ("common.labels.street", "Labels", "Common"),
            ("common.labels.city", "Labels", "Common"),
            ("common.labels.state", "Labels", "Common"),
            ("common.labels.country", "Labels", "Common"),
            ("common.labels.zip_code", "Labels", "Common"),
            ("common.labels.postal_code", "Labels", "Common"),
            
            // Common Labels - Status & Type
            ("common.labels.status", "Labels", "Common"),
            ("common.labels.type", "Labels", "Common"),
            ("common.labels.category", "Labels", "Common"),
            ("common.labels.subcategory", "Labels", "Common"),
            ("common.labels.group", "Labels", "Common"),
            ("common.labels.parent", "Labels", "Common"),
            ("common.labels.children", "Labels", "Common"),
            
            // Common Labels - Date & Time
            ("common.labels.date", "Labels", "Common"),
            ("common.labels.time", "Labels", "Common"),
            ("common.labels.datetime", "Labels", "Common"),
            ("common.labels.start_date", "Labels", "Common"),
            ("common.labels.end_date", "Labels", "Common"),
            ("common.labels.due_date", "Labels", "Common"),
            ("common.labels.created_at", "Labels", "Common"),
            ("common.labels.updated_at", "Labels", "Common"),
            ("common.labels.deleted_at", "Labels", "Common"),
            ("common.labels.created_by", "Labels", "Common"),
            ("common.labels.updated_by", "Labels", "Common"),
            ("common.labels.deleted_by", "Labels", "Common"),
            
            // Common Labels - State
            ("common.labels.active", "Labels", "Common"),
            ("common.labels.inactive", "Labels", "Common"),
            ("common.labels.enabled", "Labels", "Common"),
            ("common.labels.disabled", "Labels", "Common"),
            ("common.labels.published", "Labels", "Common"),
            ("common.labels.draft", "Labels", "Common"),
            ("common.labels.pending", "Labels", "Common"),
            ("common.labels.approved", "Labels", "Common"),
            ("common.labels.rejected", "Labels", "Common"),
            ("common.labels.completed", "Labels", "Common"),
            ("common.labels.cancelled", "Labels", "Common"),
            ("common.labels.archived", "Labels", "Common"),
            
            // Common Labels - Boolean
            ("common.labels.yes", "Labels", "Common"),
            ("common.labels.no", "Labels", "Common"),
            ("common.labels.true", "Labels", "Common"),
            ("common.labels.false", "Labels", "Common"),
            ("common.labels.all", "Labels", "Common"),
            ("common.labels.none", "Labels", "Common"),
            ("common.labels.any", "Labels", "Common"),
            ("common.labels.other", "Labels", "Common"),
            ("common.labels.unknown", "Labels", "Common"),
            ("common.labels.default", "Labels", "Common"),
            ("common.labels.custom", "Labels", "Common"),
            
            // Common Labels - Actions
            ("common.labels.select", "Labels", "Common"),
            ("common.labels.search", "Labels", "Common"),
            ("common.labels.filter", "Labels", "Common"),
            ("common.labels.sort", "Labels", "Common"),
            ("common.labels.sort_by", "Labels", "Common"),
            ("common.labels.order", "Labels", "Common"),
            ("common.labels.ascending", "Labels", "Common"),
            ("common.labels.descending", "Labels", "Common"),
            ("common.labels.actions", "Labels", "Common"),
            ("common.labels.details", "Labels", "Common"),
            ("common.labels.summary", "Labels", "Common"),
            ("common.labels.info", "Labels", "Common"),
            ("common.labels.information", "Labels", "Common"),
            ("common.labels.settings", "Labels", "Common"),
            ("common.labels.preferences", "Labels", "Common"),
            ("common.labels.options", "Labels", "Common"),
            ("common.labels.configuration", "Labels", "Common"),
            
            // Common Labels - Numeric
            ("common.labels.total", "Labels", "Common"),
            ("common.labels.subtotal", "Labels", "Common"),
            ("common.labels.count", "Labels", "Common"),
            ("common.labels.quantity", "Labels", "Common"),
            ("common.labels.amount", "Labels", "Common"),
            ("common.labels.price", "Labels", "Common"),
            ("common.labels.cost", "Labels", "Common"),
            ("common.labels.discount", "Labels", "Common"),
            ("common.labels.tax", "Labels", "Common"),
            ("common.labels.currency", "Labels", "Common"),
            ("common.labels.percentage", "Labels", "Common"),
            
            // Common Labels - Data
            ("common.labels.key", "Labels", "Common"),
            ("common.labels.value", "Labels", "Common"),
            ("common.labels.code", "Labels", "Common"),
            ("common.labels.id", "Labels", "Common"),
            ("common.labels.reference", "Labels", "Common"),
            ("common.labels.number", "Labels", "Common"),
            ("common.labels.icon", "Labels", "Common"),
            ("common.labels.image", "Labels", "Common"),
            ("common.labels.logo", "Labels", "Common"),
            ("common.labels.avatar", "Labels", "Common"),
            ("common.labels.photo", "Labels", "Common"),
            ("common.labels.file", "Labels", "Common"),
            ("common.labels.attachment", "Labels", "Common"),
            ("common.labels.document", "Labels", "Common"),
            ("common.labels.folder", "Labels", "Common"),
            ("common.labels.path", "Labels", "Common"),
            ("common.labels.filename", "Labels", "Common"),
            
            // Common Labels - Order & Position
            ("common.labels.sort_order", "Labels", "Common"),
            ("common.labels.size", "Labels", "Common"),
            ("common.labels.priority", "Labels", "Common"),
            ("common.labels.level", "Labels", "Common"),
            ("common.labels.rank", "Labels", "Common"),
            ("common.labels.position", "Labels", "Common"),
            ("common.labels.index", "Labels", "Common"),
            ("common.labels.sequence", "Labels", "Common"),
            
            // Common Labels - Visibility
            ("common.labels.required", "Labels", "Common"),
            ("common.labels.optional", "Labels", "Common"),
            ("common.labels.mandatory", "Labels", "Common"),
            ("common.labels.readonly", "Labels", "Common"),
            ("common.labels.editable", "Labels", "Common"),
            ("common.labels.visible", "Labels", "Common"),
            ("common.labels.hidden", "Labels", "Common"),
            ("common.labels.public", "Labels", "Common"),
            ("common.labels.private", "Labels", "Common"),
            ("common.labels.internal", "Labels", "Common"),
            ("common.labels.external", "Labels", "Common"),
            ("common.labels.system", "Labels", "Common"),
            ("common.labels.user", "Labels", "Common"),
            
            // Common Labels - System
            ("common.labels.version", "Labels", "Common"),
            ("common.labels.revision", "Labels", "Common"),
            ("common.labels.language", "Labels", "Common"),
            ("common.labels.locale", "Labels", "Common"),
            ("common.labels.timezone", "Labels", "Common"),
            ("common.labels.format", "Labels", "Common"),
            ("common.labels.template", "Labels", "Common"),
            ("common.labels.layout", "Labels", "Common"),
            ("common.labels.theme", "Labels", "Common"),
            ("common.labels.style", "Labels", "Common"),
            ("common.labels.mode", "Labels", "Common"),
            
            // Common Labels - Communication
            ("common.labels.from", "Labels", "Common"),
            ("common.labels.to", "Labels", "Common"),
            ("common.labels.subject", "Labels", "Common"),
            ("common.labels.message", "Labels", "Common"),
            ("common.labels.body", "Labels", "Common"),
            ("common.labels.content", "Labels", "Common"),
            ("common.labels.text", "Labels", "Common"),
            ("common.labels.notes", "Labels", "Common"),
            ("common.labels.comments", "Labels", "Common"),
            ("common.labels.remarks", "Labels", "Common"),
            ("common.labels.tags", "Labels", "Common"),
            ("common.labels.keywords", "Labels", "Common"),
            
            // Common Labels - Progress
            ("common.labels.progress", "Labels", "Common"),
            ("common.labels.percentage_complete", "Labels", "Common"),
            ("common.labels.remaining", "Labels", "Common"),
            ("common.labels.elapsed", "Labels", "Common"),
            ("common.labels.duration", "Labels", "Common"),
            
            // Common Buttons - CRUD
            ("common.buttons.save", "Buttons", "Common"),
            ("common.buttons.save_and_close", "Buttons", "Common"),
            ("common.buttons.save_and_new", "Buttons", "Common"),
            ("common.buttons.save_changes", "Buttons", "Common"),
            ("common.buttons.save_draft", "Buttons", "Common"),
            ("common.buttons.cancel", "Buttons", "Common"),
            ("common.buttons.delete", "Buttons", "Common"),
            ("common.buttons.remove", "Buttons", "Common"),
            ("common.buttons.edit", "Buttons", "Common"),
            ("common.buttons.modify", "Buttons", "Common"),
            ("common.buttons.create", "Buttons", "Common"),
            ("common.buttons.add", "Buttons", "Common"),
            ("common.buttons.add_new", "Buttons", "Common"),
            ("common.buttons.new", "Buttons", "Common"),
            ("common.buttons.update", "Buttons", "Common"),
            ("common.buttons.submit", "Buttons", "Common"),
            ("common.buttons.send", "Buttons", "Common"),
            ("common.buttons.post", "Buttons", "Common"),
            ("common.buttons.publish", "Buttons", "Common"),
            ("common.buttons.unpublish", "Buttons", "Common"),
            
            // Common Buttons - Approval
            ("common.buttons.approve", "Buttons", "Common"),
            ("common.buttons.reject", "Buttons", "Common"),
            ("common.buttons.accept", "Buttons", "Common"),
            ("common.buttons.decline", "Buttons", "Common"),
            ("common.buttons.confirm", "Buttons", "Common"),
            ("common.buttons.verify", "Buttons", "Common"),
            ("common.buttons.validate", "Buttons", "Common"),
            
            // Common Buttons - Form
            ("common.buttons.reset", "Buttons", "Common"),
            ("common.buttons.clear", "Buttons", "Common"),
            ("common.buttons.clear_all", "Buttons", "Common"),
            ("common.buttons.close", "Buttons", "Common"),
            ("common.buttons.open", "Buttons", "Common"),
            ("common.buttons.ok", "Buttons", "Common"),
            ("common.buttons.yes", "Buttons", "Common"),
            ("common.buttons.no", "Buttons", "Common"),
            ("common.buttons.apply", "Buttons", "Common"),
            ("common.buttons.done", "Buttons", "Common"),
            
            // Common Buttons - Navigation
            ("common.buttons.back", "Buttons", "Common"),
            ("common.buttons.forward", "Buttons", "Common"),
            ("common.buttons.next", "Buttons", "Common"),
            ("common.buttons.previous", "Buttons", "Common"),
            ("common.buttons.first", "Buttons", "Common"),
            ("common.buttons.last", "Buttons", "Common"),
            ("common.buttons.finish", "Buttons", "Common"),
            ("common.buttons.complete", "Buttons", "Common"),
            ("common.buttons.continue", "Buttons", "Common"),
            ("common.buttons.skip", "Buttons", "Common"),
            
            // Common Buttons - Data Operations
            ("common.buttons.refresh", "Buttons", "Common"),
            ("common.buttons.reload", "Buttons", "Common"),
            ("common.buttons.retry", "Buttons", "Common"),
            ("common.buttons.undo", "Buttons", "Common"),
            ("common.buttons.redo", "Buttons", "Common"),
            ("common.buttons.revert", "Buttons", "Common"),
            ("common.buttons.restore", "Buttons", "Common"),
            ("common.buttons.archive", "Buttons", "Common"),
            ("common.buttons.unarchive", "Buttons", "Common"),
            
            // Common Buttons - Import/Export
            ("common.buttons.export", "Buttons", "Common"),
            ("common.buttons.export_all", "Buttons", "Common"),
            ("common.buttons.export_selected", "Buttons", "Common"),
            ("common.buttons.export_csv", "Buttons", "Common"),
            ("common.buttons.export_excel", "Buttons", "Common"),
            ("common.buttons.export_pdf", "Buttons", "Common"),
            ("common.buttons.import", "Buttons", "Common"),
            ("common.buttons.download", "Buttons", "Common"),
            ("common.buttons.upload", "Buttons", "Common"),
            ("common.buttons.browse", "Buttons", "Common"),
            ("common.buttons.choose_file", "Buttons", "Common"),
            ("common.buttons.attach", "Buttons", "Common"),
            ("common.buttons.detach", "Buttons", "Common"),
            
            // Common Buttons - View
            ("common.buttons.view", "Buttons", "Common"),
            ("common.buttons.view_all", "Buttons", "Common"),
            ("common.buttons.view_more", "Buttons", "Common"),
            ("common.buttons.view_less", "Buttons", "Common"),
            ("common.buttons.view_details", "Buttons", "Common"),
            ("common.buttons.show", "Buttons", "Common"),
            ("common.buttons.hide", "Buttons", "Common"),
            ("common.buttons.expand", "Buttons", "Common"),
            ("common.buttons.collapse", "Buttons", "Common"),
            ("common.buttons.expand_all", "Buttons", "Common"),
            ("common.buttons.collapse_all", "Buttons", "Common"),
            ("common.buttons.preview", "Buttons", "Common"),
            ("common.buttons.print", "Buttons", "Common"),
            
            // Common Buttons - Management
            ("common.buttons.manage", "Buttons", "Common"),
            ("common.buttons.configure", "Buttons", "Common"),
            ("common.buttons.customize", "Buttons", "Common"),
            ("common.buttons.enable", "Buttons", "Common"),
            ("common.buttons.disable", "Buttons", "Common"),
            ("common.buttons.activate", "Buttons", "Common"),
            ("common.buttons.deactivate", "Buttons", "Common"),
            ("common.buttons.lock", "Buttons", "Common"),
            ("common.buttons.unlock", "Buttons", "Common"),
            
            // Common Buttons - Auth
            ("common.buttons.login", "Buttons", "Common"),
            ("common.buttons.logout", "Buttons", "Common"),
            ("common.buttons.sign_in", "Buttons", "Common"),
            ("common.buttons.sign_out", "Buttons", "Common"),
            ("common.buttons.sign_up", "Buttons", "Common"),
            ("common.buttons.register", "Buttons", "Common"),
            ("common.buttons.forgot_password", "Buttons", "Common"),
            ("common.buttons.reset_password", "Buttons", "Common"),
            ("common.buttons.change_password", "Buttons", "Common"),
            
            // Common Buttons - Selection
            ("common.buttons.toggle_all", "Buttons", "Common"),
            ("common.buttons.select_all", "Buttons", "Common"),
            ("common.buttons.deselect_all", "Buttons", "Common"),
            ("common.buttons.invert_selection", "Buttons", "Common"),
            
            // Common Buttons - Clipboard
            ("common.buttons.copy", "Buttons", "Common"),
            ("common.buttons.cut", "Buttons", "Common"),
            ("common.buttons.paste", "Buttons", "Common"),
            ("common.buttons.duplicate", "Buttons", "Common"),
            ("common.buttons.clone", "Buttons", "Common"),
            
            // Common Buttons - Move
            ("common.buttons.move", "Buttons", "Common"),
            ("common.buttons.move_up", "Buttons", "Common"),
            ("common.buttons.move_down", "Buttons", "Common"),
            ("common.buttons.sort", "Buttons", "Common"),
            ("common.buttons.filter", "Buttons", "Common"),
            ("common.buttons.search", "Buttons", "Common"),
            ("common.buttons.find", "Buttons", "Common"),
            ("common.buttons.replace", "Buttons", "Common"),
            
            // Common Buttons - Actions
            ("common.buttons.go", "Buttons", "Common"),
            ("common.buttons.run", "Buttons", "Common"),
            ("common.buttons.execute", "Buttons", "Common"),
            ("common.buttons.start", "Buttons", "Common"),
            ("common.buttons.stop", "Buttons", "Common"),
            ("common.buttons.pause", "Buttons", "Common"),
            ("common.buttons.resume", "Buttons", "Common"),
            ("common.buttons.play", "Buttons", "Common"),
            
            // Common Buttons - Communication
            ("common.buttons.share", "Buttons", "Common"),
            ("common.buttons.email", "Buttons", "Common"),
            ("common.buttons.help", "Buttons", "Common"),
            ("common.buttons.info", "Buttons", "Common"),
            ("common.buttons.about", "Buttons", "Common"),
            ("common.buttons.contact", "Buttons", "Common"),
            ("common.buttons.feedback", "Buttons", "Common"),
            ("common.buttons.report", "Buttons", "Common"),
            
            // Common Buttons - Sync
            ("common.buttons.sync", "Buttons", "Common"),
            ("common.buttons.connect", "Buttons", "Common"),
            ("common.buttons.disconnect", "Buttons", "Common"),
            ("common.buttons.test", "Buttons", "Common"),
            ("common.buttons.test_connection", "Buttons", "Common"),
            
            // Common Buttons - More
            ("common.buttons.more", "Buttons", "Common"),
            ("common.buttons.less", "Buttons", "Common"),
            ("common.buttons.more_options", "Buttons", "Common"),
            ("common.buttons.advanced", "Buttons", "Common"),
            ("common.buttons.details", "Buttons", "Common"),
            ("common.buttons.settings", "Buttons", "Common"),
            ("common.buttons.options", "Buttons", "Common"),
            ("common.buttons.learn_more", "Buttons", "Common"),
            ("common.buttons.read_more", "Buttons", "Common"),
            
            // Common Buttons - Social
            ("common.buttons.like", "Buttons", "Common"),
            ("common.buttons.unlike", "Buttons", "Common"),
            ("common.buttons.favorite", "Buttons", "Common"),
            ("common.buttons.unfavorite", "Buttons", "Common"),
            ("common.buttons.bookmark", "Buttons", "Common"),
            ("common.buttons.follow", "Buttons", "Common"),
            ("common.buttons.unfollow", "Buttons", "Common"),
            ("common.buttons.subscribe", "Buttons", "Common"),
            ("common.buttons.unsubscribe", "Buttons", "Common"),
            
            // Common Buttons - Notifications
            ("common.buttons.mark_as_read", "Buttons", "Common"),
            ("common.buttons.mark_as_unread", "Buttons", "Common"),
            ("common.buttons.mark_all_read", "Buttons", "Common"),
            ("common.buttons.dismiss", "Buttons", "Common"),
            ("common.buttons.ignore", "Buttons", "Common"),
            
            // Common Buttons - Assignment
            ("common.buttons.assign", "Buttons", "Common"),
            ("common.buttons.unassign", "Buttons", "Common"),
            ("common.buttons.reassign", "Buttons", "Common"),
            ("common.buttons.transfer", "Buttons", "Common"),
            ("common.buttons.merge", "Buttons", "Common"),
            ("common.buttons.split", "Buttons", "Common"),
            ("common.buttons.link", "Buttons", "Common"),
            ("common.buttons.unlink", "Buttons", "Common"),
            
            // Common Buttons - History
            ("common.buttons.history", "Buttons", "Common"),
            ("common.buttons.audit", "Buttons", "Common"),
            ("common.buttons.log", "Buttons", "Common"),
            ("common.buttons.compare", "Buttons", "Common"),
            
            // Navigation
            ("nav.sections.main", "Navigation", "Admin"),
            ("nav.sections.management", "Navigation", "Admin"),
            ("nav.sections.system", "Navigation", "Admin"),
            ("nav.sections.help", "Navigation", "Admin"),
            ("nav.menu.dashboard", "Navigation", "Admin"),
            ("nav.menu.calendar", "Navigation", "Admin"),
            ("nav.menu.identity", "Navigation", "Admin"),
            ("nav.menu.users", "Navigation", "Admin"),
            ("nav.menu.roles", "Navigation", "Admin"),
            ("nav.menu.claims", "Navigation", "Admin"),
            ("nav.menu.permissions", "Navigation", "Admin"),
            ("nav.menu.features", "Navigation", "Admin"),
            ("nav.menu.role_permissions", "Navigation", "Admin"),
            ("nav.menu.tenants", "Navigation", "Admin"),
            ("nav.menu.list_of_values", "Navigation", "Admin"),
            ("nav.menu.settings", "Navigation", "Admin"),
            ("nav.menu.notifications", "Navigation", "Admin"),
            ("nav.menu.theme", "Navigation", "Admin"),
            ("nav.menu.localization", "Navigation", "Admin"),
            ("nav.menu.languages", "Navigation", "Admin"),
            ("nav.menu.resources", "Navigation", "Admin"),
            ("nav.menu.translations", "Navigation", "Admin"),
            ("nav.menu.email_templates", "Navigation", "Admin"),
            ("nav.menu.documentation", "Navigation", "Admin"),
            ("nav.menu.overview", "Navigation", "Admin"),
            ("nav.menu.getting_started", "Navigation", "Admin"),
            ("nav.menu.configuration", "Navigation", "Admin"),
            ("nav.menu.architecture", "Navigation", "Admin"),
            ("nav.menu.database", "Navigation", "Admin"),
            ("nav.menu.security", "Navigation", "Admin"),
            ("nav.menu.api_reference", "Navigation", "Admin"),
            ("nav.menu.back_to_site", "Navigation", "Admin"),
            
            // Page Titles
            ("titles.dashboard", "Titles", "Admin"),
            ("titles.users", "Titles", "Admin"),
            ("titles.user_details", "Titles", "Admin"),
            ("titles.create_user", "Titles", "Admin"),
            ("titles.edit_user", "Titles", "Admin"),
            ("titles.roles", "Titles", "Admin"),
            ("titles.role_details", "Titles", "Admin"),
            ("titles.create_role", "Titles", "Admin"),
            ("titles.edit_role", "Titles", "Admin"),
            ("titles.claims", "Titles", "Admin"),
            ("titles.claim_types", "Titles", "Admin"),
            ("titles.create_claim_type", "Titles", "Admin"),
            ("titles.edit_claim_type", "Titles", "Admin"),
            ("titles.role_claims", "Titles", "Admin"),
            ("titles.permissions", "Titles", "Admin"),
            ("titles.features", "Titles", "Admin"),
            ("titles.create_feature", "Titles", "Admin"),
            ("titles.edit_feature", "Titles", "Admin"),
            ("titles.role_permissions", "Titles", "Admin"),
            ("titles.settings", "Titles", "Admin"),
            ("titles.tenants", "Titles", "Admin"),
            ("titles.notifications", "Titles", "Admin"),
            ("titles.localization", "Titles", "Admin"),
            ("titles.languages", "Titles", "Admin"),
            ("titles.resources", "Titles", "Admin"),
            ("titles.translations", "Titles", "Admin"),
            ("titles.email_templates", "Titles", "Admin"),
            ("titles.list_of_values", "Titles", "Admin"),
            ("titles.profile", "Titles", "Admin"),
            ("titles.change_password", "Titles", "Admin"),
            ("titles.general_settings", "Titles", "Admin"),
            ("titles.security_settings", "Titles", "Admin"),
            ("titles.email_settings", "Titles", "Admin"),
            ("titles.tenant_details", "Titles", "Admin"),
            ("titles.create_tenant", "Titles", "Admin"),
            ("titles.edit_tenant", "Titles", "Admin"),
            
            // Messages
            ("messages.save_success", "Messages", "Common"),
            ("messages.create_success", "Messages", "Common"),
            ("messages.update_success", "Messages", "Common"),
            ("messages.delete_success", "Messages", "Common"),
            ("messages.confirm_delete", "Messages", "Common"),
            ("messages.no_records_found", "Messages", "Common"),
            ("messages.loading", "Messages", "Common"),
            ("messages.processing", "Messages", "Common"),
            ("messages.please_wait", "Messages", "Common"),
            ("messages.welcome", "Messages", "Common"),
            ("messages.goodbye", "Messages", "Common"),
            ("messages.import_success", "Messages", "Common"),
            ("messages.export_success", "Messages", "Common"),
            ("messages.operation_success", "Messages", "Common"),
            ("messages.confirm_action", "Messages", "Common"),
            
            // Errors
            ("errors.general", "Errors", "Common"),
            ("errors.not_found", "Errors", "Common"),
            ("errors.unauthorized", "Errors", "Common"),
            ("errors.forbidden", "Errors", "Common"),
            ("errors.validation_failed", "Errors", "Common"),
            ("errors.save_failed", "Errors", "Common"),
            ("errors.delete_failed", "Errors", "Common"),
            ("errors.required_field", "Errors", "Common"),
            ("errors.invalid_email", "Errors", "Common"),
            ("errors.bad_request", "Errors", "Common"),
            ("errors.server_error", "Errors", "Common"),
            ("errors.duplicate_entry", "Errors", "Common"),
            ("errors.invalid_input", "Errors", "Common"),
            ("errors.invalid_password", "Errors", "Common"),
            ("errors.password_mismatch", "Errors", "Common"),
            ("errors.session_expired", "Errors", "Common"),
            ("errors.network_error", "Errors", "Common"),
            
            // Validation
            ("validation.required", "Validation", "Common"),
            ("validation.min_length", "Validation", "Common"),
            ("validation.max_length", "Validation", "Common"),
            ("validation.email", "Validation", "Common"),
            ("validation.phone", "Validation", "Common"),
            ("validation.url", "Validation", "Common"),
            ("validation.number", "Validation", "Common"),
            ("validation.integer", "Validation", "Common"),
            ("validation.decimal", "Validation", "Common"),
            ("validation.date", "Validation", "Common"),
            ("validation.range", "Validation", "Common"),
            ("validation.regex", "Validation", "Common"),
            ("validation.compare", "Validation", "Common"),
            ("validation.unique", "Validation", "Common"),
            
            // Identity Labels
            ("identity.username", "Labels", "Identity"),
            ("identity.password", "Labels", "Identity"),
            ("identity.confirm_password", "Labels", "Identity"),
            ("identity.first_name", "Labels", "Identity"),
            ("identity.last_name", "Labels", "Identity"),
            ("identity.full_name", "Labels", "Identity"),
            ("identity.display_name", "Labels", "Identity"),
            ("identity.email_address", "Labels", "Identity"),
            ("identity.phone_number", "Labels", "Identity"),
            ("identity.role", "Labels", "Identity"),
            ("identity.roles", "Labels", "Identity"),
            ("identity.claim", "Labels", "Identity"),
            ("identity.claims", "Labels", "Identity"),
            ("identity.permission", "Labels", "Identity"),
            ("identity.permissions", "Labels", "Identity"),
            ("identity.last_login", "Labels", "Identity"),
            ("identity.current_password", "Labels", "Identity"),
            ("identity.new_password", "Labels", "Identity"),
            ("identity.account_locked", "Labels", "Identity"),
            ("identity.email_confirmed", "Labels", "Identity"),
            ("identity.two_factor_enabled", "Labels", "Identity"),
            
            // Admin Labels
            ("admin.admin_panel", "Labels", "Admin"),
            ("admin.welcome_admin", "Labels", "Admin"),
            ("admin.total_users", "Labels", "Admin"),
            ("admin.total_roles", "Labels", "Admin"),
            ("admin.total_tenants", "Labels", "Admin"),
            ("admin.active_users", "Labels", "Admin"),
            ("admin.pending_approvals", "Labels", "Admin"),
            ("admin.system_health", "Labels", "Admin"),
            ("admin.recent_activity", "Labels", "Admin"),
            ("admin.quick_actions", "Labels", "Admin"),
            ("admin.system_info", "Labels", "Admin"),
            ("admin.all_rights_reserved", "Labels", "Admin"),
            
            // Placeholders
            ("placeholders.enter_name", "Placeholders", "Common"),
            ("placeholders.enter_email", "Placeholders", "Common"),
            ("placeholders.enter_password", "Placeholders", "Common"),
            ("placeholders.enter_search", "Placeholders", "Common"),
            ("placeholders.select_option", "Placeholders", "Common"),
            ("placeholders.type_to_search", "Placeholders", "Common"),
            ("placeholders.enter_description", "Placeholders", "Common"),
            ("placeholders.enter_key", "Placeholders", "Common"),
            ("placeholders.enter_value", "Placeholders", "Common"),
        ];
    }
    
    private static Dictionary<string, string> GetDefaultEnglishTranslations()
    {
        return new Dictionary<string, string>
        {
            // Common Labels - Basic
            ["common.labels.name"] = "Name",
            ["common.labels.title"] = "Title",
            ["common.labels.description"] = "Description",
            ["common.labels.email"] = "Email",
            ["common.labels.phone"] = "Phone",
            ["common.labels.mobile"] = "Mobile",
            ["common.labels.fax"] = "Fax",
            ["common.labels.website"] = "Website",
            ["common.labels.url"] = "URL",
            ["common.labels.address"] = "Address",
            ["common.labels.street"] = "Street",
            ["common.labels.city"] = "City",
            ["common.labels.state"] = "State",
            ["common.labels.country"] = "Country",
            ["common.labels.zip_code"] = "Zip Code",
            ["common.labels.postal_code"] = "Postal Code",
            
            // Common Labels - Status & Type
            ["common.labels.status"] = "Status",
            ["common.labels.type"] = "Type",
            ["common.labels.category"] = "Category",
            ["common.labels.subcategory"] = "Subcategory",
            ["common.labels.group"] = "Group",
            ["common.labels.parent"] = "Parent",
            ["common.labels.children"] = "Children",
            
            // Common Labels - Date & Time
            ["common.labels.date"] = "Date",
            ["common.labels.time"] = "Time",
            ["common.labels.datetime"] = "Date & Time",
            ["common.labels.start_date"] = "Start Date",
            ["common.labels.end_date"] = "End Date",
            ["common.labels.due_date"] = "Due Date",
            ["common.labels.created_at"] = "Created At",
            ["common.labels.updated_at"] = "Updated At",
            ["common.labels.deleted_at"] = "Deleted At",
            ["common.labels.created_by"] = "Created By",
            ["common.labels.updated_by"] = "Updated By",
            ["common.labels.deleted_by"] = "Deleted By",
            
            // Common Labels - State
            ["common.labels.active"] = "Active",
            ["common.labels.inactive"] = "Inactive",
            ["common.labels.enabled"] = "Enabled",
            ["common.labels.disabled"] = "Disabled",
            ["common.labels.published"] = "Published",
            ["common.labels.draft"] = "Draft",
            ["common.labels.pending"] = "Pending",
            ["common.labels.approved"] = "Approved",
            ["common.labels.rejected"] = "Rejected",
            ["common.labels.completed"] = "Completed",
            ["common.labels.cancelled"] = "Cancelled",
            ["common.labels.archived"] = "Archived",
            
            // Common Labels - Boolean
            ["common.labels.yes"] = "Yes",
            ["common.labels.no"] = "No",
            ["common.labels.true"] = "True",
            ["common.labels.false"] = "False",
            ["common.labels.all"] = "All",
            ["common.labels.none"] = "None",
            ["common.labels.any"] = "Any",
            ["common.labels.other"] = "Other",
            ["common.labels.unknown"] = "Unknown",
            ["common.labels.default"] = "Default",
            ["common.labels.custom"] = "Custom",
            
            // Common Labels - Actions
            ["common.labels.select"] = "Select",
            ["common.labels.search"] = "Search",
            ["common.labels.filter"] = "Filter",
            ["common.labels.sort"] = "Sort",
            ["common.labels.sort_by"] = "Sort By",
            ["common.labels.order"] = "Order",
            ["common.labels.ascending"] = "Ascending",
            ["common.labels.descending"] = "Descending",
            ["common.labels.actions"] = "Actions",
            ["common.labels.details"] = "Details",
            ["common.labels.summary"] = "Summary",
            ["common.labels.info"] = "Info",
            ["common.labels.information"] = "Information",
            ["common.labels.settings"] = "Settings",
            ["common.labels.preferences"] = "Preferences",
            ["common.labels.options"] = "Options",
            ["common.labels.configuration"] = "Configuration",
            
            // Common Labels - Numeric
            ["common.labels.total"] = "Total",
            ["common.labels.subtotal"] = "Subtotal",
            ["common.labels.count"] = "Count",
            ["common.labels.quantity"] = "Quantity",
            ["common.labels.amount"] = "Amount",
            ["common.labels.price"] = "Price",
            ["common.labels.cost"] = "Cost",
            ["common.labels.discount"] = "Discount",
            ["common.labels.tax"] = "Tax",
            ["common.labels.currency"] = "Currency",
            ["common.labels.percentage"] = "Percentage",
            
            // Common Labels - Data
            ["common.labels.key"] = "Key",
            ["common.labels.value"] = "Value",
            ["common.labels.code"] = "Code",
            ["common.labels.id"] = "ID",
            ["common.labels.reference"] = "Reference",
            ["common.labels.number"] = "Number",
            ["common.labels.icon"] = "Icon",
            ["common.labels.image"] = "Image",
            ["common.labels.logo"] = "Logo",
            ["common.labels.avatar"] = "Avatar",
            ["common.labels.photo"] = "Photo",
            ["common.labels.file"] = "File",
            ["common.labels.attachment"] = "Attachment",
            ["common.labels.document"] = "Document",
            ["common.labels.folder"] = "Folder",
            ["common.labels.path"] = "Path",
            ["common.labels.filename"] = "Filename",
            
            // Common Labels - Order & Position
            ["common.labels.sort_order"] = "Sort Order",
            ["common.labels.priority"] = "Priority",
            ["common.labels.level"] = "Level",
            ["common.labels.rank"] = "Rank",
            ["common.labels.position"] = "Position",
            ["common.labels.index"] = "Index",
            ["common.labels.sequence"] = "Sequence",
            
            // Common Labels - Visibility
            ["common.labels.required"] = "Required",
            ["common.labels.optional"] = "Optional",
            ["common.labels.mandatory"] = "Mandatory",
            ["common.labels.readonly"] = "Read Only",
            ["common.labels.editable"] = "Editable",
            ["common.labels.visible"] = "Visible",
            ["common.labels.hidden"] = "Hidden",
            ["common.labels.public"] = "Public",
            ["common.labels.private"] = "Private",
            ["common.labels.internal"] = "Internal",
            ["common.labels.external"] = "External",
            ["common.labels.system"] = "System",
            ["common.labels.user"] = "User",
            
            // Common Labels - System
            ["common.labels.version"] = "Version",
            ["common.labels.revision"] = "Revision",
            ["common.labels.language"] = "Language",
            ["common.labels.locale"] = "Locale",
            ["common.labels.timezone"] = "Timezone",
            ["common.labels.format"] = "Format",
            ["common.labels.template"] = "Template",
            ["common.labels.layout"] = "Layout",
            ["common.labels.theme"] = "Theme",
            ["common.labels.style"] = "Style",
            ["common.labels.mode"] = "Mode",
            
            // Common Labels - Communication
            ["common.labels.from"] = "From",
            ["common.labels.to"] = "To",
            ["common.labels.subject"] = "Subject",
            ["common.labels.message"] = "Message",
            ["common.labels.body"] = "Body",
            ["common.labels.content"] = "Content",
            ["common.labels.text"] = "Text",
            ["common.labels.notes"] = "Notes",
            ["common.labels.comments"] = "Comments",
            ["common.labels.remarks"] = "Remarks",
            ["common.labels.tags"] = "Tags",
            ["common.labels.keywords"] = "Keywords",
            
            // Common Labels - Progress
            ["common.labels.progress"] = "Progress",
            ["common.labels.percentage_complete"] = "% Complete",
            ["common.labels.remaining"] = "Remaining",
            ["common.labels.elapsed"] = "Elapsed",
            ["common.labels.duration"] = "Duration",
            
            // Common Buttons - CRUD
            ["common.buttons.save"] = "Save",
            ["common.buttons.save_and_close"] = "Save & Close",
            ["common.buttons.save_and_new"] = "Save & New",
            ["common.buttons.save_changes"] = "Save Changes",
            ["common.buttons.save_draft"] = "Save Draft",
            ["common.buttons.cancel"] = "Cancel",
            ["common.buttons.delete"] = "Delete",
            ["common.buttons.remove"] = "Remove",
            ["common.buttons.edit"] = "Edit",
            ["common.buttons.modify"] = "Modify",
            ["common.buttons.create"] = "Create",
            ["common.buttons.add"] = "Add",
            ["common.buttons.add_new"] = "Add New",
            ["common.buttons.new"] = "New",
            ["common.buttons.update"] = "Update",
            ["common.buttons.submit"] = "Submit",
            ["common.buttons.send"] = "Send",
            ["common.buttons.post"] = "Post",
            ["common.buttons.publish"] = "Publish",
            ["common.buttons.unpublish"] = "Unpublish",
            
            // Common Buttons - Approval
            ["common.buttons.approve"] = "Approve",
            ["common.buttons.reject"] = "Reject",
            ["common.buttons.accept"] = "Accept",
            ["common.buttons.decline"] = "Decline",
            ["common.buttons.confirm"] = "Confirm",
            ["common.buttons.verify"] = "Verify",
            ["common.buttons.validate"] = "Validate",
            
            // Common Buttons - Form
            ["common.buttons.reset"] = "Reset",
            ["common.buttons.clear"] = "Clear",
            ["common.buttons.clear_all"] = "Clear All",
            ["common.buttons.close"] = "Close",
            ["common.buttons.open"] = "Open",
            ["common.buttons.ok"] = "OK",
            ["common.buttons.yes"] = "Yes",
            ["common.buttons.no"] = "No",
            ["common.buttons.apply"] = "Apply",
            ["common.buttons.done"] = "Done",
            
            // Common Buttons - Navigation
            ["common.buttons.back"] = "Back",
            ["common.buttons.forward"] = "Forward",
            ["common.buttons.next"] = "Next",
            ["common.buttons.previous"] = "Previous",
            ["common.buttons.first"] = "First",
            ["common.buttons.last"] = "Last",
            ["common.buttons.finish"] = "Finish",
            ["common.buttons.complete"] = "Complete",
            ["common.buttons.continue"] = "Continue",
            ["common.buttons.skip"] = "Skip",
            
            // Common Buttons - Data Operations
            ["common.buttons.refresh"] = "Refresh",
            ["common.buttons.reload"] = "Reload",
            ["common.buttons.retry"] = "Retry",
            ["common.buttons.undo"] = "Undo",
            ["common.buttons.redo"] = "Redo",
            ["common.buttons.revert"] = "Revert",
            ["common.buttons.restore"] = "Restore",
            ["common.buttons.archive"] = "Archive",
            ["common.buttons.unarchive"] = "Unarchive",
            
            // Common Buttons - Import/Export
            ["common.buttons.export"] = "Export",
            ["common.buttons.export_all"] = "Export All",
            ["common.buttons.export_selected"] = "Export Selected",
            ["common.buttons.export_csv"] = "Export CSV",
            ["common.buttons.export_excel"] = "Export Excel",
            ["common.buttons.export_pdf"] = "Export PDF",
            ["common.buttons.import"] = "Import",
            ["common.buttons.download"] = "Download",
            ["common.buttons.upload"] = "Upload",
            ["common.buttons.browse"] = "Browse",
            ["common.buttons.choose_file"] = "Choose File",
            ["common.buttons.attach"] = "Attach",
            ["common.buttons.detach"] = "Detach",
            
            // Common Buttons - View
            ["common.buttons.view"] = "View",
            ["common.buttons.view_all"] = "View All",
            ["common.buttons.view_more"] = "View More",
            ["common.buttons.view_less"] = "View Less",
            ["common.buttons.view_details"] = "View Details",
            ["common.buttons.show"] = "Show",
            ["common.buttons.hide"] = "Hide",
            ["common.buttons.expand"] = "Expand",
            ["common.buttons.collapse"] = "Collapse",
            ["common.buttons.expand_all"] = "Expand All",
            ["common.buttons.collapse_all"] = "Collapse All",
            ["common.buttons.preview"] = "Preview",
            ["common.buttons.print"] = "Print",
            
            // Common Buttons - Management
            ["common.buttons.manage"] = "Manage",
            ["common.buttons.configure"] = "Configure",
            ["common.buttons.customize"] = "Customize",
            ["common.buttons.enable"] = "Enable",
            ["common.buttons.disable"] = "Disable",
            ["common.buttons.activate"] = "Activate",
            ["common.buttons.deactivate"] = "Deactivate",
            ["common.buttons.lock"] = "Lock",
            ["common.buttons.unlock"] = "Unlock",
            
            // Common Buttons - Auth
            ["common.buttons.login"] = "Login",
            ["common.buttons.logout"] = "Logout",
            ["common.buttons.sign_in"] = "Sign In",
            ["common.buttons.sign_out"] = "Sign Out",
            ["common.buttons.sign_up"] = "Sign Up",
            ["common.buttons.register"] = "Register",
            ["common.buttons.forgot_password"] = "Forgot Password",
            ["common.buttons.reset_password"] = "Reset Password",
            ["common.buttons.change_password"] = "Change Password",
            
            // Common Buttons - Selection
            ["common.buttons.toggle_all"] = "Toggle All",
            ["common.buttons.select_all"] = "Select All",
            ["common.buttons.deselect_all"] = "Deselect All",
            ["common.buttons.invert_selection"] = "Invert Selection",
            
            // Common Buttons - Clipboard
            ["common.buttons.copy"] = "Copy",
            ["common.buttons.cut"] = "Cut",
            ["common.buttons.paste"] = "Paste",
            ["common.buttons.duplicate"] = "Duplicate",
            ["common.buttons.clone"] = "Clone",
            
            // Common Buttons - Move
            ["common.buttons.move"] = "Move",
            ["common.buttons.move_up"] = "Move Up",
            ["common.buttons.move_down"] = "Move Down",
            ["common.buttons.sort"] = "Sort",
            ["common.buttons.filter"] = "Filter",
            ["common.buttons.search"] = "Search",
            ["common.buttons.find"] = "Find",
            ["common.buttons.replace"] = "Replace",
            
            // Common Buttons - Actions
            ["common.buttons.go"] = "Go",
            ["common.buttons.run"] = "Run",
            ["common.buttons.execute"] = "Execute",
            ["common.buttons.start"] = "Start",
            ["common.buttons.stop"] = "Stop",
            ["common.buttons.pause"] = "Pause",
            ["common.buttons.resume"] = "Resume",
            ["common.buttons.play"] = "Play",
            
            // Common Buttons - Communication
            ["common.buttons.share"] = "Share",
            ["common.buttons.email"] = "Email",
            ["common.buttons.help"] = "Help",
            ["common.buttons.info"] = "Info",
            ["common.buttons.about"] = "About",
            ["common.buttons.contact"] = "Contact",
            ["common.buttons.feedback"] = "Feedback",
            ["common.buttons.report"] = "Report",
            
            // Common Buttons - Sync
            ["common.buttons.sync"] = "Sync",
            ["common.buttons.connect"] = "Connect",
            ["common.buttons.disconnect"] = "Disconnect",
            ["common.buttons.test"] = "Test",
            ["common.buttons.test_connection"] = "Test Connection",
            
            // Common Buttons - More
            ["common.buttons.more"] = "More",
            ["common.buttons.less"] = "Less",
            ["common.buttons.more_options"] = "More Options",
            ["common.buttons.advanced"] = "Advanced",
            ["common.buttons.details"] = "Details",
            ["common.buttons.settings"] = "Settings",
            ["common.buttons.options"] = "Options",
            ["common.buttons.learn_more"] = "Learn More",
            ["common.buttons.read_more"] = "Read More",
            
            // Common Buttons - Social
            ["common.buttons.like"] = "Like",
            ["common.buttons.unlike"] = "Unlike",
            ["common.buttons.favorite"] = "Favorite",
            ["common.buttons.unfavorite"] = "Unfavorite",
            ["common.buttons.bookmark"] = "Bookmark",
            ["common.buttons.follow"] = "Follow",
            ["common.buttons.unfollow"] = "Unfollow",
            ["common.buttons.subscribe"] = "Subscribe",
            ["common.buttons.unsubscribe"] = "Unsubscribe",
            
            // Common Buttons - Notifications
            ["common.buttons.mark_as_read"] = "Mark as Read",
            ["common.buttons.mark_as_unread"] = "Mark as Unread",
            ["common.buttons.mark_all_read"] = "Mark All Read",
            ["common.buttons.dismiss"] = "Dismiss",
            ["common.buttons.ignore"] = "Ignore",
            
            // Common Buttons - Assignment
            ["common.buttons.assign"] = "Assign",
            ["common.buttons.unassign"] = "Unassign",
            ["common.buttons.reassign"] = "Reassign",
            ["common.buttons.transfer"] = "Transfer",
            ["common.buttons.merge"] = "Merge",
            ["common.buttons.split"] = "Split",
            ["common.buttons.link"] = "Link",
            ["common.buttons.unlink"] = "Unlink",
            
            // Common Buttons - History
            ["common.buttons.history"] = "History",
            ["common.buttons.audit"] = "Audit",
            ["common.buttons.log"] = "Log",
            ["common.buttons.compare"] = "Compare",
            
            // Navigation
            ["nav.sections.main"] = "Main",
            ["nav.sections.management"] = "Management",
            ["nav.sections.system"] = "System",
            ["nav.sections.help"] = "Help",
            ["nav.menu.dashboard"] = "Dashboard",
            ["nav.menu.calendar"] = "Calendar",
            ["nav.menu.identity"] = "Identity",
            ["nav.menu.users"] = "Users",
            ["nav.menu.roles"] = "Roles",
            ["nav.menu.claims"] = "Claims",
            ["nav.menu.permissions"] = "Permissions",
            ["nav.menu.features"] = "Features",
            ["nav.menu.role_permissions"] = "Role Permissions",
            ["nav.menu.tenants"] = "Tenants",
            ["nav.menu.list_of_values"] = "List of Values",
            ["nav.menu.settings"] = "Settings",
            ["nav.menu.notifications"] = "Notifications",
            ["nav.menu.theme"] = "Theme",
            ["nav.menu.localization"] = "Localization",
            ["nav.menu.languages"] = "Languages",
            ["nav.menu.resources"] = "Resources",
            ["nav.menu.translations"] = "Translations",
            ["nav.menu.email_templates"] = "Email Templates",
            ["nav.menu.documentation"] = "Documentation",
            ["nav.menu.overview"] = "Overview",
            ["nav.menu.getting_started"] = "Getting Started",
            ["nav.menu.configuration"] = "Configuration",
            ["nav.menu.architecture"] = "Architecture",
            ["nav.menu.database"] = "Database",
            ["nav.menu.security"] = "Security",
            ["nav.menu.api_reference"] = "API Reference",
            ["nav.menu.back_to_site"] = "Back to Site",
            
            // Page Titles
            ["titles.dashboard"] = "Dashboard",
            ["titles.users"] = "Users",
            ["titles.user_details"] = "User Details",
            ["titles.create_user"] = "Create User",
            ["titles.edit_user"] = "Edit User",
            ["titles.roles"] = "Roles",
            ["titles.role_details"] = "Role Details",
            ["titles.create_role"] = "Create Role",
            ["titles.edit_role"] = "Edit Role",
            ["titles.claims"] = "Claims",
            ["titles.claim_types"] = "Claim Types",
            ["titles.create_claim_type"] = "Create Claim Type",
            ["titles.edit_claim_type"] = "Edit Claim Type",
            ["titles.role_claims"] = "Role Claims",
            ["titles.permissions"] = "Permissions",
            ["titles.features"] = "Features",
            ["titles.create_feature"] = "Create Feature",
            ["titles.edit_feature"] = "Edit Feature",
            ["titles.role_permissions"] = "Role Permissions",
            ["titles.settings"] = "Settings",
            ["titles.tenants"] = "Tenants",
            ["titles.notifications"] = "Notifications",
            ["titles.localization"] = "Localization",
            ["titles.languages"] = "Languages",
            ["titles.resources"] = "Resources",
            ["titles.translations"] = "Translations",
            ["titles.email_templates"] = "Email Templates",
            ["titles.list_of_values"] = "List of Values",
            ["titles.profile"] = "Profile",
            ["titles.change_password"] = "Change Password",
            ["titles.general_settings"] = "General Settings",
            ["titles.security_settings"] = "Security Settings",
            ["titles.email_settings"] = "Email Settings",
            ["titles.tenant_details"] = "Tenant Details",
            ["titles.create_tenant"] = "Create Tenant",
            ["titles.edit_tenant"] = "Edit Tenant",
            
            // Messages
            ["messages.save_success"] = "{0} saved successfully",
            ["messages.create_success"] = "{0} created successfully",
            ["messages.update_success"] = "{0} updated successfully",
            ["messages.delete_success"] = "{0} deleted successfully",
            ["messages.confirm_delete"] = "Are you sure you want to delete this item?",
            ["messages.no_records_found"] = "No records found",
            ["messages.loading"] = "Loading...",
            ["messages.processing"] = "Processing...",
            ["messages.please_wait"] = "Please wait...",
            ["messages.welcome"] = "Welcome, {0}!",
            ["messages.goodbye"] = "Goodbye!",
            ["messages.import_success"] = "{0} imported successfully",
            ["messages.export_success"] = "{0} exported successfully",
            ["messages.operation_success"] = "Operation completed successfully",
            ["messages.confirm_action"] = "Are you sure you want to perform this action?",
            
            // Errors
            ["errors.general"] = "An error occurred. Please try again.",
            ["errors.not_found"] = "{0} not found",
            ["errors.unauthorized"] = "You are not authorized to perform this action",
            ["errors.forbidden"] = "Access denied",
            ["errors.validation_failed"] = "Validation failed. Please check your input.",
            ["errors.save_failed"] = "Failed to save {0}",
            ["errors.delete_failed"] = "Failed to delete {0}",
            ["errors.required_field"] = "{0} is required",
            ["errors.invalid_email"] = "Invalid email address",
            ["errors.bad_request"] = "Bad request",
            ["errors.server_error"] = "Server error occurred",
            ["errors.duplicate_entry"] = "{0} already exists",
            ["errors.invalid_input"] = "Invalid input",
            ["errors.invalid_password"] = "Invalid password",
            ["errors.password_mismatch"] = "Passwords do not match",
            ["errors.session_expired"] = "Your session has expired. Please login again.",
            ["errors.network_error"] = "Network error. Please check your connection.",
            
            // Validation
            ["validation.required"] = "{0} is required",
            ["validation.min_length"] = "{0} must be at least {1} characters",
            ["validation.max_length"] = "{0} must not exceed {1} characters",
            ["validation.email"] = "Please enter a valid email address",
            ["validation.phone"] = "Please enter a valid phone number",
            ["validation.url"] = "Please enter a valid URL",
            ["validation.number"] = "Please enter a valid number",
            ["validation.integer"] = "Please enter a valid integer",
            ["validation.decimal"] = "Please enter a valid decimal number",
            ["validation.date"] = "Please enter a valid date",
            ["validation.range"] = "{0} must be between {1} and {2}",
            ["validation.regex"] = "{0} format is invalid",
            ["validation.compare"] = "{0} and {1} do not match",
            ["validation.unique"] = "{0} must be unique",
            
            // Identity Labels
            ["identity.username"] = "Username",
            ["identity.password"] = "Password",
            ["identity.confirm_password"] = "Confirm Password",
            ["identity.first_name"] = "First Name",
            ["identity.last_name"] = "Last Name",
            ["identity.full_name"] = "Full Name",
            ["identity.display_name"] = "Display Name",
            ["identity.email_address"] = "Email Address",
            ["identity.phone_number"] = "Phone Number",
            ["identity.role"] = "Role",
            ["identity.roles"] = "Roles",
            ["identity.claim"] = "Claim",
            ["identity.claims"] = "Claims",
            ["identity.permission"] = "Permission",
            ["identity.permissions"] = "Permissions",
            ["identity.last_login"] = "Last Login",
            ["identity.current_password"] = "Current Password",
            ["identity.new_password"] = "New Password",
            ["identity.account_locked"] = "Account Locked",
            ["identity.email_confirmed"] = "Email Confirmed",
            ["identity.two_factor_enabled"] = "Two-Factor Enabled",
            
            // Admin Labels
            ["admin.admin_panel"] = "Admin Panel",
            ["admin.welcome_admin"] = "Welcome, Admin!",
            ["admin.total_users"] = "Total Users",
            ["admin.total_roles"] = "Total Roles",
            ["admin.total_tenants"] = "Total Tenants",
            ["admin.active_users"] = "Active Users",
            ["admin.pending_approvals"] = "Pending Approvals",
            ["admin.system_health"] = "System Health",
            ["admin.recent_activity"] = "Recent Activity",
            ["admin.quick_actions"] = "Quick Actions",
            ["admin.system_info"] = "System Info",
            ["admin.all_rights_reserved"] = "All rights reserved.",
            
            // Placeholders
            ["placeholders.enter_name"] = "Enter name",
            ["placeholders.enter_email"] = "Enter email",
            ["placeholders.enter_password"] = "Enter password",
            ["placeholders.enter_search"] = "Search...",
            ["placeholders.select_option"] = "Select an option",
            ["placeholders.type_to_search"] = "Type to search...",
            ["placeholders.enter_description"] = "Enter description",
            ["placeholders.enter_key"] = "Enter key",
            ["placeholders.enter_value"] = "Enter value",
            
            // Additional Labels
            ["common.labels.size"] = "Size",
            ["common.labels.priority"] = "Priority",
            ["common.labels.level"] = "Level",
        };
    }

    private void SaveAll()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(Path.Combine(_dataPath, "languages.json"), JsonSerializer.Serialize(_languages, options));
        File.WriteAllText(Path.Combine(_dataPath, "resources.json"), JsonSerializer.Serialize(_resources, options));
        File.WriteAllText(Path.Combine(_dataPath, "translations.json"), JsonSerializer.Serialize(_translations, options));
    }

    #region Languages

    public async Task<List<Language>> GetAllLanguagesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _languages.OrderBy(l => l.SortOrder).ToList();
    }

    public async Task<List<Language>> GetActiveLanguagesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _languages.Where(l => l.IsActive).OrderBy(l => l.SortOrder).ToList();
    }

    public async Task<Language?> GetLanguageByCodeAsync(string code, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Language> CreateLanguageAsync(Language language, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _languages.Add(language);
        SaveAll();
        return language;
    }

    public async Task<Language> UpdateLanguageAsync(Language language, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _languages.FirstOrDefault(l => l.Code.Equals(language.Code, StringComparison.OrdinalIgnoreCase));
        if (existing == null) throw new InvalidOperationException("Language not found");
        
        existing.Name = language.Name;
        existing.NativeName = language.NativeName;
        existing.Icon = language.Icon;
        existing.IsRtl = language.IsRtl;
        existing.IsDefault = language.IsDefault;
        existing.IsActive = language.IsActive;
        existing.SortOrder = language.SortOrder;
        
        if (language.IsDefault)
        {
            foreach (var l in _languages.Where(x => x.Code != language.Code))
                l.IsDefault = false;
        }
        
        SaveAll();
        return existing;
    }

    public async Task DeleteLanguageAsync(string code, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var language = _languages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (language == null) return;
        if (language.IsDefault) throw new InvalidOperationException("Cannot delete default language");
        
        _languages.Remove(language);
        _translations.RemoveAll(t => t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        SaveAll();
    }

    public async Task<string> GetDefaultLanguageCodeAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _languages.FirstOrDefault(l => l.IsDefault)?.Code ?? "en";
    }

    #endregion

    #region Resources

    public async Task<List<Resource>> GetAllResourcesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _resources.Where(r => r.IsActive).OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<List<Resource>> GetResourceTreeAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var all = _resources.Where(r => r.IsActive).ToList();
        var roots = all.Where(r => r.ParentId == null).OrderBy(r => r.SortOrder).ToList();
        
        foreach (var root in roots)
        {
            BuildResourceTree(root, all);
            root.Translations = _translations.Where(t => t.ResourceId == root.Id).ToList();
        }
        
        return roots;
    }

    private void BuildResourceTree(Resource parent, List<Resource> all)
    {
        parent.Children = all.Where(r => r.ParentId == parent.Id).OrderBy(r => r.SortOrder).ToList();
        parent.Translations = _translations.Where(t => t.ResourceId == parent.Id).ToList();
        foreach (var child in parent.Children)
            BuildResourceTree(child, all);
    }

    public async Task<List<Resource>> GetResourcesByModuleAsync(string module, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _resources.Where(r => r.Module?.Equals(module, StringComparison.OrdinalIgnoreCase) == true && r.IsActive)
            .OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<List<Resource>> GetResourcesByCategoryAsync(string category, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _resources.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && r.IsActive)
            .OrderBy(r => r.SortOrder).ToList();
    }

    public async Task<Resource?> GetResourceByIdAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource != null)
            resource.Translations = _translations.Where(t => t.ResourceId == id).ToList();
        return resource;
    }

    public async Task<Resource?> GetResourceByKeyAsync(string key, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var resource = _resources.FirstOrDefault(r => r.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (resource != null)
            resource.Translations = _translations.Where(t => t.ResourceId == resource.Id).ToList();
        return resource;
    }

    public async Task<Resource> CreateResourceAsync(Resource resource, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        resource.Id = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        _resources.Add(resource);
        SaveAll();
        return resource;
    }

    public async Task<Resource> UpdateResourceAsync(Resource resource, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _resources.FirstOrDefault(r => r.Id == resource.Id);
        if (existing == null) throw new InvalidOperationException("Resource not found");
        
        existing.Key = resource.Key;
        existing.ParentId = resource.ParentId;
        existing.Category = resource.Category;
        existing.Module = resource.Module;
        existing.Description = resource.Description;
        existing.MaxLength = resource.MaxLength;
        existing.SupportsPluralForms = resource.SupportsPluralForms;
        existing.Placeholders = resource.Placeholders;
        existing.SortOrder = resource.SortOrder;
        existing.IsActive = resource.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        SaveAll();
        return existing;
    }

    public async Task DeleteResourceAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource == null) return;
        if (resource.IsSystem) throw new InvalidOperationException("Cannot delete system resource");
        
        // Delete children recursively
        var children = _resources.Where(r => r.ParentId == id).ToList();
        foreach (var child in children)
            await DeleteResourceAsync(child.Id, ct);
        
        _resources.Remove(resource);
        _translations.RemoveAll(t => t.ResourceId == id);
        SaveAll();
    }

    #endregion

    #region Translations

    public async Task<ResourceTranslation?> GetTranslationAsync(Guid resourceId, string languageCode, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _translations.FirstOrDefault(t => 
            t.ResourceId == resourceId && t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ResourceTranslation?> GetTranslationByKeyAsync(string resourceKey, string languageCode, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var resource = _resources.FirstOrDefault(r => r.Key.Equals(resourceKey, StringComparison.OrdinalIgnoreCase));
        if (resource == null) return null;
        
        return _translations.FirstOrDefault(t => 
            t.ResourceId == resource.Id && t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ResourceTranslation>> GetTranslationsForResourceAsync(Guid resourceId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _translations.Where(t => t.ResourceId == resourceId).ToList();
    }

    public async Task<List<ResourceTranslation>> GetTranslationsForLanguageAsync(string languageCode, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _translations.Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<ResourceTranslation> SetTranslationAsync(Guid resourceId, string languageCode, string value, string? pluralValue = null, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _translations.FirstOrDefault(t => 
            t.ResourceId == resourceId && t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        
        if (existing != null)
        {
            existing.Value = value;
            existing.PluralValue = pluralValue;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Status = TranslationStatus.Draft;
        }
        else
        {
            existing = new ResourceTranslation
            {
                ResourceId = resourceId,
                LanguageCode = languageCode,
                Value = value,
                PluralValue = pluralValue,
                Status = TranslationStatus.Draft
            };
            _translations.Add(existing);
        }
        
        SaveAll();
        return existing;
    }

    public async Task<ResourceTranslation> UpdateTranslationStatusAsync(Guid translationId, TranslationStatus status, string? notes = null, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var translation = _translations.FirstOrDefault(t => t.Id == translationId);
        if (translation == null) throw new InvalidOperationException("Translation not found");
        
        translation.Status = status;
        translation.ReviewNotes = notes;
        translation.IsReviewed = status == TranslationStatus.Approved || status == TranslationStatus.Published;
        translation.UpdatedAt = DateTime.UtcNow;
        
        SaveAll();
        return translation;
    }

    public async Task DeleteTranslationAsync(Guid translationId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _translations.RemoveAll(t => t.Id == translationId);
        SaveAll();
    }

    public async Task<string> GetValueAsync(string resourceKey, string languageCode, CancellationToken ct = default)
    {
        var translation = await GetTranslationByKeyAsync(resourceKey, languageCode, ct);
        if (translation != null) return translation.Value;
        
        // Fallback to default language
        var defaultLang = await GetDefaultLanguageCodeAsync(ct);
        if (!languageCode.Equals(defaultLang, StringComparison.OrdinalIgnoreCase))
        {
            translation = await GetTranslationByKeyAsync(resourceKey, defaultLang, ct);
            if (translation != null) return translation.Value;
        }
        
        return resourceKey; // Return key if no translation found
    }

    public async Task<string> GetValueAsync(string resourceKey, string languageCode, params object[] args)
    {
        var value = await GetValueAsync(resourceKey, languageCode);
        return args.Length > 0 ? string.Format(value, args) : value;
    }

    public async Task<Dictionary<string, string>> GetAllTranslationsAsync(string languageCode, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var result = new Dictionary<string, string>();
        
        foreach (var translation in _translations.Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)))
        {
            var resource = _resources.FirstOrDefault(r => r.Id == translation.ResourceId);
            if (resource != null)
                result[resource.Key] = translation.Value;
        }
        
        return result;
    }

    public async Task<TranslationStats> GetTranslationStatsAsync(string languageCode, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var langTranslations = _translations.Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)).ToList();
        
        return new TranslationStats
        {
            LanguageCode = languageCode,
            TotalResources = _resources.Count(r => r.IsActive),
            TranslatedCount = langTranslations.Count,
            PendingCount = langTranslations.Count(t => t.Status == TranslationStatus.Pending),
            ApprovedCount = langTranslations.Count(t => t.Status == TranslationStatus.Approved || t.Status == TranslationStatus.Published),
            NeedsReviewCount = langTranslations.Count(t => t.Status == TranslationStatus.NeedsReview)
        };
    }

    #endregion

    #region Import/Export

    public async Task<string> ExportAsync(string? languageCode = null, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var data = new
        {
            Languages = _languages,
            Resources = _resources,
            Translations = languageCode == null 
                ? _translations 
                : _translations.Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase)).ToList()
        };
        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<int> ImportAsync(string json, bool overwrite = false, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        int count = 0;
        
        if (data.TryGetProperty("Resources", out var resources))
        {
            var importedResources = JsonSerializer.Deserialize<List<Resource>>(resources.GetRawText()) ?? [];
            foreach (var resource in importedResources)
            {
                var existing = _resources.FirstOrDefault(r => r.Key.Equals(resource.Key, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    _resources.Add(resource);
                    count++;
                }
                else if (overwrite && !existing.IsSystem)
                {
                    existing.Category = resource.Category;
                    existing.Module = resource.Module;
                    existing.Description = resource.Description;
                    count++;
                }
            }
        }
        
        if (data.TryGetProperty("Translations", out var translations))
        {
            var importedTranslations = JsonSerializer.Deserialize<List<ResourceTranslation>>(translations.GetRawText()) ?? [];
            foreach (var translation in importedTranslations)
            {
                var existing = _translations.FirstOrDefault(t => 
                    t.ResourceId == translation.ResourceId && 
                    t.LanguageCode.Equals(translation.LanguageCode, StringComparison.OrdinalIgnoreCase));
                
                if (existing == null)
                {
                    _translations.Add(translation);
                    count++;
                }
                else if (overwrite)
                {
                    existing.Value = translation.Value;
                    existing.PluralValue = translation.PluralValue;
                    count++;
                }
            }
        }
        
        SaveAll();
        return count;
    }

    #endregion
}
