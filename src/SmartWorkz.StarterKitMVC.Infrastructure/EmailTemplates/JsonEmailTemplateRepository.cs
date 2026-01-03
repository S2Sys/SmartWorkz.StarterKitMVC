using System.Text.Json;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;
using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// JSON file-based implementation of email template repository.
/// Stores templates in App_Data/EmailTemplates/ directory.
/// </summary>
public sealed class JsonEmailTemplateRepository : IEmailTemplateRepository
{
    private readonly string _basePath;
    private readonly string _templatesPath;
    private readonly string _sectionsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the repository.
    /// </summary>
    /// <param name="basePath">Base path for storage (defaults to App_Data/EmailTemplates).</param>
    public JsonEmailTemplateRepository(string? basePath = null)
    {
        _basePath = basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "EmailTemplates");
        _templatesPath = Path.Combine(_basePath, "templates");
        _sectionsPath = Path.Combine(_basePath, "sections");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_templatesPath);
        Directory.CreateDirectory(_sectionsPath);
    }

    #region Templates

    public async Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var templates = new List<EmailTemplate>();
        var files = Directory.GetFiles(_templatesPath, "*.json");
        
        foreach (var file in files)
        {
            var template = await ReadTemplateFileAsync(file, cancellationToken);
            if (template != null)
            {
                if (tenantId == null || template.TenantId == null || template.TenantId == tenantId)
                {
                    templates.Add(template);
                }
            }
        }
        
        return templates.OrderBy(t => t.Name).ToList();
    }

    public async Task<EmailTemplate?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetTemplateFilePath(id);
        if (!File.Exists(filePath))
            return null;
            
        return await ReadTemplateFileAsync(filePath, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var all = await GetAllTemplatesAsync(tenantId, cancellationToken);
        return all.Where(t => string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<EmailTemplate> SaveTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            template.UpdatedAt = DateTime.UtcNow;
            var filePath = GetTemplateFilePath(template.Id);
            var json = JsonSerializer.Serialize(template, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
            return template;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteTemplateAsync(string id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var filePath = GetTemplateFilePath(id);
            if (!File.Exists(filePath))
                return false;
                
            File.Delete(filePath);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<bool> TemplateExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetTemplateFilePath(id);
        return Task.FromResult(File.Exists(filePath));
    }

    private string GetTemplateFilePath(string id) => Path.Combine(_templatesPath, $"{SanitizeFileName(id)}.json");

    private async Task<EmailTemplate?> ReadTemplateFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<EmailTemplate>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Sections

    public async Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var sections = new List<EmailTemplateSection>();
        var files = Directory.GetFiles(_sectionsPath, "*.json");
        
        foreach (var file in files)
        {
            var section = await ReadSectionFileAsync(file, cancellationToken);
            if (section != null)
            {
                if (type == null || section.Type == type)
                {
                    if (tenantId == null || section.TenantId == null || section.TenantId == tenantId)
                    {
                        sections.Add(section);
                    }
                }
            }
        }
        
        return sections.OrderBy(s => s.Type).ThenBy(s => s.Name).ToList();
    }

    public async Task<EmailTemplateSection?> GetSectionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filePath = GetSectionFilePath(id);
        if (!File.Exists(filePath))
            return null;
            
        return await ReadSectionFileAsync(filePath, cancellationToken);
    }

    public async Task<EmailTemplateSection?> GetDefaultSectionAsync(SectionType type, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var sections = await GetAllSectionsAsync(type, tenantId, cancellationToken);
        return sections.FirstOrDefault(s => s.IsDefault) ?? sections.FirstOrDefault();
    }

    public async Task<EmailTemplateSection> SaveSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            section.UpdatedAt = DateTime.UtcNow;
            
            // If this section is being set as default, unset other defaults of same type
            if (section.IsDefault)
            {
                var allSections = await GetAllSectionsAsync(section.Type, section.TenantId, cancellationToken);
                foreach (var other in allSections.Where(s => s.Id != section.Id && s.IsDefault))
                {
                    other.IsDefault = false;
                    var otherPath = GetSectionFilePath(other.Id);
                    var otherJson = JsonSerializer.Serialize(other, _jsonOptions);
                    await File.WriteAllTextAsync(otherPath, otherJson, cancellationToken);
                }
            }
            
            var filePath = GetSectionFilePath(section.Id);
            var json = JsonSerializer.Serialize(section, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
            return section;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteSectionAsync(string id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var filePath = GetSectionFilePath(id);
            if (!File.Exists(filePath))
                return false;
                
            File.Delete(filePath);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private string GetSectionFilePath(string id) => Path.Combine(_sectionsPath, $"{SanitizeFileName(id)}.json");

    private async Task<EmailTemplateSection?> ReadSectionFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<EmailTemplateSection>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}
