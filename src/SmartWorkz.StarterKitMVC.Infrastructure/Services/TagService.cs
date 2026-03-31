using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly SharedDbContext _context;

    public TagService(SharedDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetTagsByEntityAsync(string tenantId, string entityType, int entityId)
    {
        return await _context.Tags
            .Where(t => t.TenantId == tenantId
                && t.EntityType == entityType
                && t.EntityId == entityId
                && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetTagsByNameAsync(string tenantId, string tagName)
    {
        return await _context.Tags
            .Where(t => t.TenantId == tenantId && t.TagName == tagName && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<Tag> CreateTagAsync(string tenantId, Tag tag)
    {
        tag.TenantId = tenantId;
        tag.CreatedAt = DateTime.UtcNow;

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return tag;
    }

    public async Task<bool> AssignTagToEntityAsync(int tagId, string entityType, int entityId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return false;

        tag.EntityType = entityType;
        tag.EntityId = entityId;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveTagFromEntityAsync(int tagId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return false;

        tag.EntityType = null;
        tag.EntityId = 0;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTagAsync(int tagId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return false;

        tag.IsDeleted = true;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
