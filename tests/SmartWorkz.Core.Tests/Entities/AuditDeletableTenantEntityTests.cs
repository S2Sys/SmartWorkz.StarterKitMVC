using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the standalone AuditDeletableTenantEntity&lt;TId&gt; and AuditDeletableTenantEntity base classes.
/// Tests combined audit trailing, soft delete, and tenant-scoped functionality.
///
/// Standalone Design:
/// AuditDeletableTenantEntity is a self-contained class that inherits from Entity&lt;TId&gt; and
/// implements IAuditable, ISoftDeletable, and ITenantScoped directly, combining audit tracking,
/// soft delete capabilities, and multi-tenant support in a single entity class without hierarchical inheritance.
/// </summary>
public class AuditDeletableTenantEntityTests
{
    #region AuditDeletableTenantEntity<int> - Audit Property Tests

    [Fact]
    public void AuditDeletableTenantEntity_StoresCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableTenantEntityInt { CreatedAt = now };

        // Act
        var createdAt = entity.CreatedAt;

        // Assert
        Assert.Equal(now, createdAt);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresCreatedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { CreatedBy = "user123" };

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal("user123", createdBy);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresUpdatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableTenantEntityInt { UpdatedAt = now };

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Equal(now, updatedAt);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresUpdatedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { UpdatedBy = "user456" };

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Equal("user456", updatedBy);
    }

    [Fact]
    public void AuditDeletableTenantEntity_CreatedByDefaultIsEmpty()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal(string.Empty, createdBy);
    }

    [Fact]
    public void AuditDeletableTenantEntity_UpdatedAtDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Null(updatedAt);
    }

    [Fact]
    public void AuditDeletableTenantEntity_UpdatedByDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Null(updatedBy);
    }

    #endregion

    #region AuditDeletableTenantEntity<int> - Soft Delete Property Tests

    [Fact]
    public void AuditDeletableTenantEntity_IsDeletedDefaultIsFalse()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresIsDeleted()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { IsDeleted = true };

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresDeletedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditDeletableTenantEntityInt { DeletedAt = now };

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Equal(now, deletedAt);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresDeletedBy()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { DeletedBy = 42 };

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Equal(42, deletedBy);
    }

    [Fact]
    public void AuditDeletableTenantEntity_DeletedAtDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Null(deletedAt);
    }

    [Fact]
    public void AuditDeletableTenantEntity_DeletedByDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Null(deletedBy);
    }

    #endregion

    #region AuditDeletableTenantEntity<int> - Tenant Property Tests

    [Fact]
    public void AuditDeletableTenantEntity_TenantIdDefaultsToEmptyString()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.Equal(string.Empty, tenantId);
    }

    [Fact]
    public void AuditDeletableTenantEntity_StoresTenantId()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { TenantId = "tenant-xyz" };

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.Equal("tenant-xyz", tenantId);
    }

    [Fact]
    public void AuditDeletableTenantEntity_TenantIdIsStringType()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.IsType<string>(tenantId);
    }

    #endregion

    #region AuditDeletableTenantEntity Combined Properties Test

    [Fact]
    public void AuditDeletableTenantEntity_AllPropertiesAreSettable()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var updateTime = DateTime.UtcNow.AddHours(1);
        var deleteTime = DateTime.UtcNow.AddHours(2);
        var entity = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            UpdatedAt = updateTime,
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deleteTime,
            DeletedBy = 99,
            TenantId = "tenant-full-stack"
        };

        // Act & Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(creationTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(updateTime, entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deleteTime, entity.DeletedAt);
        Assert.Equal(99, entity.DeletedBy);
        Assert.Equal("tenant-full-stack", entity.TenantId);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsIAuditable()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<IAuditable>(entity);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsISoftDeletable()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<ISoftDeletable>(entity);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsITenantScoped()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<ITenantScoped>(entity);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsIAuditableInterface_HasAuditProperties()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt() as IAuditable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("CreatedAt") != null);
        Assert.True(entity.GetType().GetProperty("CreatedBy") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedAt") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedBy") != null);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsISoftDeletableInterface_HasDeleteProperties()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt() as ISoftDeletable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("IsDeleted") != null);
        Assert.True(entity.GetType().GetProperty("DeletedAt") != null);
        Assert.True(entity.GetType().GetProperty("DeletedBy") != null);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ImplementsITenantScopedInterface_HasTenantProperty()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt() as ITenantScoped;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("TenantId") != null);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuditDeletableTenantEntity_InheritsFromEntity()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void AuditDeletableTenantEntity_InheritsIdProperty()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt { Id = 99 };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(99, id);
    }

    [Fact]
    public void AuditDeletableTenantEntity_PreservesEntityEqualitySemantics()
    {
        // Arrange
        var entity1 = new TestAuditDeletableTenantEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = true,
            TenantId = "tenant-1"
        };
        var entity2 = new TestAuditDeletableTenantEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "bob",
            IsDeleted = false,
            TenantId = "tenant-2"
        };

        // Act & Assert
        // Two entities with same Id are equal, regardless of audit, delete, or tenant properties
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void AuditDeletableTenantEntity_DifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestAuditDeletableTenantEntityInt { Id = 5 };
        var entity2 = new TestAuditDeletableTenantEntityInt { Id = 10 };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
    }

    [Fact]
    public void AuditDeletableTenantEntity_CanBeUsedInHashSet()
    {
        // Arrange
        var entity1 = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1",
            IsDeleted = false,
            TenantId = "tenant-1"
        };
        var entity2 = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "user2",
            IsDeleted = true,
            TenantId = "tenant-2"
        };
        var entity3 = new TestAuditDeletableTenantEntityInt { Id = 2 };
        var set = new HashSet<TestAuditDeletableTenantEntityInt>();

        // Act
        set.Add(entity1);
        set.Add(entity2); // Should not add duplicate (same Id)
        set.Add(entity3);

        // Assert
        Assert.Equal(2, set.Count);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void AuditDeletableTenantEntity_SupportsGuidPrimaryKey()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new TestAuditDeletableTenantEntityGuid
        {
            Id = guid,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user123",
            IsDeleted = false,
            TenantId = "tenant-guid"
        };

        // Act & Assert
        Assert.Equal(guid, entity.Id);
        Assert.Equal("user123", entity.CreatedBy);
        Assert.False(entity.IsDeleted);
        Assert.Equal("tenant-guid", entity.TenantId);
    }

    [Fact]
    public void AuditDeletableTenantEntity_SupportsStringPrimaryKey()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityString
        {
            Id = "CODE123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user456",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddHours(1),
            DeletedBy = 7,
            TenantId = "tenant-string"
        };

        // Act & Assert
        Assert.Equal("CODE123", entity.Id);
        Assert.Equal("user456", entity.CreatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(7, entity.DeletedBy);
        Assert.Equal("tenant-string", entity.TenantId);
    }

    #endregion

    #region Convenience Class Tests

    [Fact]
    public void AuditDeletableTenantEntity_ConvenienceClass_HasIntId()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntity { Id = 100, CreatedAt = DateTime.UtcNow };

        // Act & Assert
        Assert.Equal(100, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ConvenienceClass_InheritsFromAuditDeletableTenantEntityInt()
    {
        // Arrange & Act
        var entity = new TestAuditDeletableTenantEntity { Id = 50 };

        // Assert
        Assert.IsAssignableFrom<AuditDeletableTenantEntity<int>>(entity);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ConvenienceClass_HasAllAuditDeleteAndTenantProperties()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var deleteTime = DateTime.UtcNow.AddHours(1);
        var entity = new TestAuditDeletableTenantEntity
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            UpdatedAt = creationTime.AddHours(1),
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deleteTime,
            DeletedBy = 42,
            TenantId = "tenant-conv"
        };

        // Act & Assert
        Assert.Equal(creationTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(creationTime.AddHours(1), entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deleteTime, entity.DeletedAt);
        Assert.Equal(42, entity.DeletedBy);
        Assert.Equal("tenant-conv", entity.TenantId);
    }

    #endregion

    #region Full-Stack Scenario Tests

    [Fact]
    public void AuditDeletableTenantEntity_FullStackScenario_CreateReadUpdateDelete()
    {
        // Arrange
        var creationTime = DateTime.UtcNow;
        var updateTime = creationTime.AddHours(2);
        var deleteTime = creationTime.AddHours(5);

        var entity = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = creationTime,
            CreatedBy = "alice",
            TenantId = "tenant-acme"
        };

        // Act - Simulate update
        entity.UpdatedAt = updateTime;
        entity.UpdatedBy = "bob";

        // Act - Simulate soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = deleteTime;
        entity.DeletedBy = 99;

        // Assert - Complete lifecycle
        Assert.Equal(1, entity.Id);
        Assert.Equal(creationTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(updateTime, entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deleteTime, entity.DeletedAt);
        Assert.Equal(99, entity.DeletedBy);
        Assert.Equal("tenant-acme", entity.TenantId);
    }

    [Fact]
    public void AuditDeletableTenantEntity_MultiTenantScenario_DifferentTenantsWithSameLogic()
    {
        // Arrange
        var tenant1Entity = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            TenantId = "tenant-alpha"
        };

        var tenant2Entity = new TestAuditDeletableTenantEntityInt
        {
            Id = 2,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "bob",
            TenantId = "tenant-beta"
        };

        // Act & Assert - Both support same features with isolated tenant context
        Assert.NotEqual(tenant1Entity.TenantId, tenant2Entity.TenantId);
        Assert.Equal("tenant-alpha", tenant1Entity.TenantId);
        Assert.Equal("tenant-beta", tenant2Entity.TenantId);
    }

    [Fact]
    public void AuditDeletableTenantEntity_ActiveEntityInTenant_DeletedFieldsAreNull()
    {
        // Arrange
        var entity = new TestAuditDeletableTenantEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = false,
            TenantId = "tenant-active"
        };

        // Act & Assert
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);
        Assert.Equal("tenant-active", entity.TenantId);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test audit-deletable-tenant entity with int primary key.
    /// </summary>
    private class TestAuditDeletableTenantEntityInt : AuditDeletableTenantEntity<int>
    {
    }

    /// <summary>
    /// Test audit-deletable-tenant entity with Guid primary key.
    /// </summary>
    private class TestAuditDeletableTenantEntityGuid : AuditDeletableTenantEntity<Guid>
    {
    }

    /// <summary>
    /// Test audit-deletable-tenant entity with string primary key.
    /// </summary>
    private class TestAuditDeletableTenantEntityString : AuditDeletableTenantEntity<string>
    {
    }

    /// <summary>
    /// Test convenience audit-deletable-tenant entity inheriting from AuditDeletableTenantEntity (int primary key).
    /// </summary>
    private class TestAuditDeletableTenantEntity : AuditDeletableTenantEntity
    {
    }

    #endregion
}
