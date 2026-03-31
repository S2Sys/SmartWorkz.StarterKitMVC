using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface ITagService
{
    Task<List<Tag>> GetTagsByEntityAsync(string tenantId, string entityType, int entityId);
    Task<List<Tag>> GetTagsByNameAsync(string tenantId, string tagName);
    Task<Tag> CreateTagAsync(string tenantId, Tag tag);
    Task<bool> AssignTagToEntityAsync(int tagId, string entityType, int entityId);
    Task<bool> RemoveTagFromEntityAsync(int tagId);
    Task<bool> DeleteTagAsync(int tagId);
}
