using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetWithRolesAsync(string userId);
    Task<User> GetWithPermissionsAsync(string userId);
    Task<List<User>> GetByTenantAsync(string tenantId);
}
