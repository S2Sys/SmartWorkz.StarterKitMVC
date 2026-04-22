using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the standalone AuditEntity&lt;TId&gt; and AuditEntity base classes.
/// Tests audit tracking functionality and IAuditable implementation.
///
/// Standalone Design:
/// AuditEntity is now a self-contained class that inherits from Entity&lt;TId&gt; and
/// implements IAuditable directly, rather than as part of a hierarchical inheritance chain.
/// This design provides clarity and independence for auditable entities.
/// </summary>
public class AuditEntityTests
{
    #region AuditEntity<int> - Basic Audit Property Tests

    [Fact]
    public void AuditEntity_StoresCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditEntityInt { CreatedAt = now };

        // Act
        var createdAt = entity.CreatedAt;

        // Assert
        Assert.Equal(now, createdAt);
    }

    [Fact]
    public void AuditEntity_StoresCreatedBy()
    {
        // Arrange
        var entity = new TestAuditEntityInt { CreatedBy = "user123" };

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal("user123", createdBy);
    }

    [Fact]
    public void AuditEntity_StoresUpdatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditEntityInt { UpdatedAt = now };

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Equal(now, updatedAt);
    }

    [Fact]
    public void AuditEntity_StoresUpdatedBy()
    {
        // Arrange
        var entity = new TestAuditEntityInt { UpdatedBy = "user456" };

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Equal("user456", updatedBy);
    }

    [Fact]
    public void AuditEntity_CreatedByDefaultIsEmpty()
    {
        // Arrange
        var entity = new TestAuditEntityInt();

        // Act
        var createdBy = entity.CreatedBy;

        // Assert
        Assert.Equal(string.Empty, createdBy);
    }

    [Fact]
    public void AuditEntity_UpdatedAtDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditEntityInt();

        // Act
        var updatedAt = entity.UpdatedAt;

        // Assert
        Assert.Null(updatedAt);
    }

    [Fact]
    public void AuditEntity_UpdatedByDefaultIsNull()
    {
        // Arrange
        var entity = new TestAuditEntityInt();

        // Act
        var updatedBy = entity.UpdatedBy;

        // Assert
        Assert.Null(updatedBy);
    }

    [Fact]
    public void AuditEntity_AllPropertiesAreSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var updateTime = DateTime.UtcNow.AddHours(1);
        var entity = new TestAuditEntityInt
        {
            Id = 1,
            CreatedAt = now,
            CreatedBy = "alice",
            UpdatedAt = updateTime,
            UpdatedBy = "bob"
        };

        // Act & Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(updateTime, entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
    }

    #endregion

    #region AuditEntity Interface Implementation Tests

    [Fact]
    public void AuditEntity_ImplementsIAuditable()
    {
        // Arrange
        var entity = new TestAuditEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<IAuditable>(entity);
    }

    [Fact]
    public void AuditEntity_ImplementsIAuditableInterface_HasAuditProperties()
    {
        // Arrange
        var entity = new TestAuditEntityInt() as IAuditable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("CreatedAt") != null);
        Assert.True(entity.GetType().GetProperty("CreatedBy") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedAt") != null);
        Assert.True(entity.GetType().GetProperty("UpdatedBy") != null);
    }

    #endregion

    #region AuditEntity Inheritance Tests

    [Fact]
    public void AuditEntity_InheritsFromEntity()
    {
        // Arrange
        var entity = new TestAuditEntityInt { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void AuditEntity_InheritsIdProperty()
    {
        // Arrange
        var entity = new TestAuditEntityInt { Id = 99 };

        // Act
        var id = entity.Id;

        // Assert
        Assert.Equal(99, id);
    }

    [Fact]
    public void AuditEntity_PreservesEntityEqualitySemantics()
    {
        // Arrange
        var entity1 = new TestAuditEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice"
        };
        var entity2 = new TestAuditEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "bob"
        };

        // Act & Assert
        // Two audit entities with same Id are equal, regardless of audit properties
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void AuditEntity_DifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestAuditEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice"
        };
        var entity2 = new TestAuditEntityInt
        {
            Id = 10,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice"
        };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
    }

    [Fact]
    public void AuditEntity_CanBeUsedInHashSet()
    {
        // Arrange
        var entity1 = new TestAuditEntityInt { Id = 1, CreatedAt = DateTime.UtcNow, CreatedBy = "user1" };
        var entity2 = new TestAuditEntityInt { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1), CreatedBy = "user2" };
        var entity3 = new TestAuditEntityInt { Id = 2, CreatedAt = DateTime.UtcNow, CreatedBy = "user1" };
        var set = new HashSet<TestAuditEntityInt>();

        // Act
        set.Add(entity1);
        set.Add(entity2); // Should not add duplicate (same Id)
        set.Add(entity3);

        // Assert
        Assert.Equal(2, set.Count);
    }

    #endregion

    #region AuditEntity Generic Type Tests

    [Fact]
    public void AuditEntity_SupportsGuidPrimaryKey()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new TestAuditEntityGuid
        {
            Id = guid,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user123"
        };

        // Act & Assert
        Assert.Equal(guid, entity.Id);
        Assert.Equal("user123", entity.CreatedBy);
    }

    [Fact]
    public void AuditEntity_SupportsStringPrimaryKey()
    {
        // Arrange
        var entity = new TestAuditEntityString
        {
            Id = "CODE123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user456"
        };

        // Act & Assert
        Assert.Equal("CODE123", entity.Id);
        Assert.Equal("user456", entity.CreatedBy);
    }

    #endregion

    #region AuditEntity Convenience Class Tests

    [Fact]
    public void AuditEntity_ConvenienceClass_HasIntId()
    {
        // Arrange
        var entity = new TestAuditEntity { Id = 100, CreatedAt = DateTime.UtcNow };

        // Act & Assert
        Assert.Equal(100, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void AuditEntity_ConvenienceClass_InheritsFromAuditEntityInt()
    {
        // Arrange & Act
        var entity = new TestAuditEntity { Id = 50 };

        // Assert
        Assert.IsAssignableFrom<AuditEntity<int>>(entity);
    }

    [Fact]
    public void AuditEntity_ConvenienceClass_HasAuditProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestAuditEntity
        {
            Id = 1,
            CreatedAt = now,
            CreatedBy = "alice",
            UpdatedAt = now.AddHours(1),
            UpdatedBy = "bob"
        };

        // Act & Assert
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(now.AddHours(1), entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
    }

    #endregion

    #region Nullable Audit Properties Tests

    [Fact]
    public void AuditEntity_UpdatedAt_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditEntityInt
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1",
            UpdatedAt = null
        };

        // Act & Assert
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void AuditEntity_UpdatedBy_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditEntityInt
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1",
            UpdatedBy = null
        };

        // Act & Assert
        Assert.Null(entity.UpdatedBy);
    }

    [Fact]
    public void AuditEntity_BothUpdateFieldsNullOnCreation()
    {
        // Arrange
        var entity = new TestAuditEntityInt
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user1"
        };

        // Act & Assert
        Assert.Null(entity.UpdatedAt);
        Assert.Null(entity.UpdatedBy);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test audit entity with int primary key.
    /// </summary>
    private class TestAuditEntityInt : AuditEntity<int>
    {
    }

    /// <summary>
    /// Test audit entity with Guid primary key.
    /// </summary>
    private class TestAuditEntityGuid : AuditEntity<Guid>
    {
    }

    /// <summary>
    /// Test audit entity with string primary key.
    /// </summary>
    private class TestAuditEntityString : AuditEntity<string>
    {
    }

    /// <summary>
    /// Test convenience audit entity inheriting from AuditEntity (int primary key).
    /// </summary>
    private class TestAuditEntity : AuditEntity
    {
    }

    #endregion
}
