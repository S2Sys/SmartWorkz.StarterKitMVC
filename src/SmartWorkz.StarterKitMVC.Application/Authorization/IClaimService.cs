using SmartWorkz.StarterKitMVC.Domain.Authorization;

namespace SmartWorkz.StarterKitMVC.Application.Authorization;

/// <summary>
/// Service interface for managing claim types and role/user claims
/// </summary>
public interface IClaimService
{
    #region Claim Types
    
    /// <summary>Get all claim types</summary>
    Task<List<ClaimType>> GetAllClaimTypesAsync(CancellationToken ct = default);
    
    /// <summary>Get active claim types</summary>
    Task<List<ClaimType>> GetActiveClaimTypesAsync(CancellationToken ct = default);
    
    /// <summary>Get claim type by ID</summary>
    Task<ClaimType?> GetClaimTypeByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>Get claim type by key</summary>
    Task<ClaimType?> GetClaimTypeByKeyAsync(string key, CancellationToken ct = default);
    
    /// <summary>Get claim types by category</summary>
    Task<List<ClaimType>> GetClaimTypesByCategoryAsync(string category, CancellationToken ct = default);
    
    /// <summary>Create a new claim type</summary>
    Task<ClaimType> CreateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default);
    
    /// <summary>Update an existing claim type</summary>
    Task<ClaimType> UpdateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default);
    
    /// <summary>Delete a claim type</summary>
    Task DeleteClaimTypeAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>Add a predefined value to a claim type</summary>
    Task<ClaimValue> AddClaimValueAsync(Guid claimTypeId, ClaimValue value, CancellationToken ct = default);
    
    /// <summary>Remove a predefined value from a claim type</summary>
    Task RemoveClaimValueAsync(Guid claimTypeId, Guid valueId, CancellationToken ct = default);
    
    #endregion
    
    #region Role Claims
    
    /// <summary>Get all claims for a role</summary>
    Task<List<RoleClaim>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default);
    
    /// <summary>Get claims of a specific type for a role</summary>
    Task<List<RoleClaim>> GetRoleClaimsByTypeAsync(string roleId, string claimType, CancellationToken ct = default);
    
    /// <summary>Add a claim to a role</summary>
    Task<RoleClaim> AddRoleClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default);
    
    /// <summary>Remove a claim from a role</summary>
    Task RemoveRoleClaimAsync(Guid claimId, CancellationToken ct = default);
    
    /// <summary>Set all claims of a type for a role (replaces existing)</summary>
    Task SetRoleClaimsAsync(string roleId, string claimType, List<string> claimValues, CancellationToken ct = default);
    
    /// <summary>Check if a role has a specific claim</summary>
    Task<bool> RoleHasClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default);
    
    /// <summary>Get all claim values of a type for multiple roles</summary>
    Task<HashSet<string>> GetClaimValuesForRolesAsync(IEnumerable<string> roleIds, string claimType, CancellationToken ct = default);
    
    #endregion
    
    #region User Claims
    
    /// <summary>Get all claims for a user</summary>
    Task<List<UserClaim>> GetUserClaimsAsync(Guid userId, CancellationToken ct = default);
    
    /// <summary>Add a claim to a user</summary>
    Task<UserClaim> AddUserClaimAsync(Guid userId, string claimType, string claimValue, CancellationToken ct = default);
    
    /// <summary>Remove a claim from a user</summary>
    Task RemoveUserClaimAsync(Guid claimId, CancellationToken ct = default);
    
    /// <summary>Set all claims of a type for a user (replaces existing)</summary>
    Task SetUserClaimsAsync(Guid userId, string claimType, List<string> claimValues, CancellationToken ct = default);
    
    #endregion
    
    #region Entity Claims Generation
    
    /// <summary>Generate standard CRUD claims for an entity</summary>
    Task<List<ClaimValue>> GenerateEntityClaimsAsync(string entity, string displayName, CancellationToken ct = default);
    
    /// <summary>Get all entities that have claims defined</summary>
    Task<List<string>> GetEntitiesWithClaimsAsync(CancellationToken ct = default);
    
    #endregion
}
