using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the TenantEntity&lt;TId&gt; and TenantEntity base classes.
/// Tests multi-tenant functionality built on top of the full entity hierarchy.
/// </summary>
public class TenantEntityTests
{
    #region TenantEntity<int> - Basic Property Tests

    [Fact]
    public void TenantEntity_TenantIdDefaultsToEmptyString()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.Equal(string.Empty, tenantId);
    }

    [Fact]
    public void TenantEntity_TenantIdCanBeSet()
    {
        // Arrange
        var entity = new TestTenantEntityInt();
        var expectedTenantId = "tenant-123";

        // Act
        entity.TenantId = expectedTenantId;

        // Assert
        Assert.Equal(expectedTenantId, entity.TenantId);
    }

    [Fact]
    public void TenantEntity_TenantIdIsStringType()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.IsType<string>(tenantId);
    }

    #endregion

    #region TenantEntity<Guid> - Generic Type Support

    [Fact]
    public void TenantEntity_WorksWithGuidPrimaryKey()
    {
        // Arrange
        var entity = new TestTenantEntityGuid
        {
            Id = Guid.NewGuid(),
            TenantId = "tenant-guid-123"
        };

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.Equal("tenant-guid-123", tenantId);
    }

    [Fact]
    public void TenantEntity_WorksWithStringPrimaryKey()
    {
        // Arrange
        var entity = new TestTenantEntityString
        {
            Id = "entity-id-123",
            TenantId = "tenant-string-123"
        };

        // Act
        var tenantId = entity.TenantId;

        // Assert
        Assert.Equal("tenant-string-123", tenantId);
    }

    #endregion

    #region TenantEntity Inheritance Chain Verification

    [Fact]
    public void TenantEntity_InheritsFromDeletableEntity()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<DeletableEntity<int>>(entity);
    }

    [Fact]
    public void TenantEntity_InheritsFromAuditEntity()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<AuditEntity<int>>(entity);
    }

    [Fact]
    public void TenantEntity_InheritsFromEntity()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
    }

    [Fact]
    public void TenantEntity_InheritanceChainIsCorrect()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act - Set properties at each inheritance level
        entity.Id = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = "alice";
        entity.UpdatedAt = DateTime.UtcNow.AddHours(1);
        entity.UpdatedBy = "bob";
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.TenantId = "tenant-123";

        // Assert - Verify all properties are accessible and maintained
        Assert.Equal(1, entity.Id);
        Assert.NotEqual(DateTime.MinValue, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.NotNull(entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);
        Assert.Equal("tenant-123", entity.TenantId);
    }

    #endregion

    #region ITenantScoped Interface Implementation

    [Fact]
    public void TenantEntity_ImplementsITenantScoped()
    {
        // Arrange
        var entity = new TestTenantEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<ITenantScoped>(entity);
    }

    [Fact]
    public void TenantEntity_ITenantScopedPropertyReturnsCorrectValue()
    {
        // Arrange
        var entity = new TestTenantEntityInt();
        var expectedTenantId = "tenant-interface-123";

        // Act
        entity.TenantId = expectedTenantId;
        ITenantScoped tenantScoped = entity;
        var actualTenantId = tenantScoped.TenantId;

        // Assert
        Assert.Equal(expectedTenantId, actualTenantId);
    }

    #endregion

    #region Convenience TenantEntity<int> Class

    [Fact]
    public void TenantEntity_ConvenienceClassUsesIntPrimaryKey()
    {
        // Arrange
        var entity = new TestTenantEntity();

        // Act
        entity.Id = 42;
        entity.TenantId = "tenant-convenience";

        // Assert
        Assert.Equal(42, entity.Id);
        Assert.Equal("tenant-convenience", entity.TenantId);
    }

    [Fact]
    public void TenantEntity_ConvenienceClassIsAbstract()
    {
        // Act & Assert
        // Verify that TenantEntity cannot be instantiated directly
        var type = typeof(TestTenantEntity).BaseType;
        Assert.NotNull(type);
        Assert.True(type!.IsAbstract);
    }

    #endregion

    #region Multi-Tenant Data Isolation Concept

    [Fact]
    public void TenantEntity_MultiTenantDataIsolation_DifferentTenantsCanCoexist()
    {
        // Arrange
        var tenant1Entity = new TestTenantEntityInt
        {
            Id = 1,
            TenantId = "tenant-alpha",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user-alpha"
        };

        var tenant2Entity = new TestTenantEntityInt
        {
            Id = 2,
            TenantId = "tenant-beta",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user-beta"
        };

        // Act & Assert - Entities maintain separate tenant identities
        Assert.Equal("tenant-alpha", tenant1Entity.TenantId);
        Assert.Equal("tenant-beta", tenant2Entity.TenantId);
        Assert.NotEqual(tenant1Entity.TenantId, tenant2Entity.TenantId);
    }

    [Fact]
    public void TenantEntity_MultiTenantDataIsolation_QueryFilteringByConcept()
    {
        // Arrange
        var entities = new[]
        {
            new TestTenantEntityInt { Id = 1, TenantId = "tenant-alpha", CreatedAt = DateTime.UtcNow, CreatedBy = "user1" },
            new TestTenantEntityInt { Id = 2, TenantId = "tenant-beta", CreatedAt = DateTime.UtcNow, CreatedBy = "user2" },
            new TestTenantEntityInt { Id = 3, TenantId = "tenant-alpha", CreatedAt = DateTime.UtcNow, CreatedBy = "user3" },
            new TestTenantEntityInt { Id = 4, TenantId = "tenant-gamma", CreatedAt = DateTime.UtcNow, CreatedBy = "user4" }
        };

        // Act - Filter entities for a specific tenant
        var tenantAlphaEntities = entities.Where(e => e.TenantId == "tenant-alpha").ToList();

        // Assert
        Assert.Equal(2, tenantAlphaEntities.Count);
        Assert.All(tenantAlphaEntities, e => Assert.Equal("tenant-alpha", e.TenantId));
    }

    [Fact]
    public void TenantEntity_SameTenantIdCanHaveDifferentIds()
    {
        // Arrange
        var entity1 = new TestTenantEntityInt
        {
            Id = 100,
            TenantId = "tenant-shared"
        };

        var entity2 = new TestTenantEntityInt
        {
            Id = 101,
            TenantId = "tenant-shared"
        };

        // Act & Assert
        Assert.Equal("tenant-shared", entity1.TenantId);
        Assert.Equal("tenant-shared", entity2.TenantId);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    #endregion

    #region Full Stack Inheritance Verification

    [Fact]
    public void TenantEntity_FullStackInheritance_Entity()
    {
        // Verify Entity → AuditEntity → DeletableEntity → TenantEntity
        var entity = new TestTenantEntityInt();

        // Act & Assert
        // From Entity<int>
        Assert.Equal(0, entity.Id);

        // From AuditEntity<int>
        Assert.Equal(DateTime.MinValue, entity.CreatedAt);
        Assert.Equal(string.Empty, entity.CreatedBy);
        Assert.Null(entity.UpdatedAt);
        Assert.Null(entity.UpdatedBy);

        // From DeletableEntity<int>
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);

        // From TenantEntity<int>
        Assert.Equal(string.Empty, entity.TenantId);
    }

    [Fact]
    public void TenantEntity_FullStackInheritance_CanSetAllProperties()
    {
        // Arrange
        var entity = new TestTenantEntityInt();
        var now = DateTime.UtcNow;

        // Act
        entity.Id = 999;
        entity.CreatedAt = now;
        entity.CreatedBy = "system";
        entity.UpdatedAt = now.AddDays(1);
        entity.UpdatedBy = "admin";
        entity.IsDeleted = true;
        entity.DeletedAt = now.AddDays(2);
        entity.DeletedBy = 1;
        entity.TenantId = "tenant-full-stack";

        // Assert
        Assert.Equal(999, entity.Id);
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal("system", entity.CreatedBy);
        Assert.Equal(now.AddDays(1), entity.UpdatedAt);
        Assert.Equal("admin", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(now.AddDays(2), entity.DeletedAt);
        Assert.Equal(1, entity.DeletedBy);
        Assert.Equal("tenant-full-stack", entity.TenantId);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test tenant entity with int primary key.
    /// </summary>
    private class TestTenantEntityInt : TenantEntity<int>
    {
    }

    /// <summary>
    /// Test tenant entity with Guid primary key.
    /// </summary>
    private class TestTenantEntityGuid : TenantEntity<Guid>
    {
    }

    /// <summary>
    /// Test tenant entity with string primary key.
    /// </summary>
    private class TestTenantEntityString : TenantEntity<string>
    {
    }

    /// <summary>
    /// Test convenience tenant entity inheriting from TenantEntity (int primary key).
    /// </summary>
    private class TestTenantEntity : TenantEntity
    {
    }

    #endregion
}
