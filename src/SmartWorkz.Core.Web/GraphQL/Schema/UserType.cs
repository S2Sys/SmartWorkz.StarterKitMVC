namespace SmartWorkz.Core.Web.GraphQL.Schema;

/// <summary>
/// GraphQL type for User entity.
/// Exposes user information with appropriate field visibility.
/// </summary>
public class UserType
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
