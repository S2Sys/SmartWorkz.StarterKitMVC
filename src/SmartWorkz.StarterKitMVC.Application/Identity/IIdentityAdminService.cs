using SmartWorkz.StarterKitMVC.Domain.Identity;

namespace SmartWorkz.StarterKitMVC.Application.Identity;

public interface IIdentityAdminService
{
    Task<IReadOnlyCollection<AppUser>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<AppRole>> GetRolesAsync(CancellationToken ct = default);
    Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
    Task RevokeRoleAsync(Guid userId, string roleName, CancellationToken ct = default);
}
