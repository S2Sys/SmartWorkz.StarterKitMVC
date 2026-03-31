using SmartWorkz.StarterKitMVC.Domain.Entities.Auth;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetWithRolesAsync(string userId);
    Task<User> GetWithPermissionsAsync(string userId);
    Task<List<User>> GetByTenantAsync(string tenantId);
}

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AuthDbContext _authContext;

    public UserRepository(AuthDbContext context) : base(context)
    {
        _authContext = context;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _authContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _authContext.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
    }

    public async Task<User> GetWithRolesAsync(string userId)
    {
        return await _authContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    public async Task<User> GetWithPermissionsAsync(string userId)
    {
        return await _authContext.Users
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    public async Task<List<User>> GetByTenantAsync(string tenantId)
    {
        return await _authContext.Users
            .Where(u => !u.IsDeleted)
            .Join(
                _authContext.TenantUsers.Where(tu => tu.TenantId == tenantId && !tu.IsDeleted),
                u => u.UserId,
                tu => tu.UserId,
                (u, tu) => u
            )
            .ToListAsync();
    }
}
