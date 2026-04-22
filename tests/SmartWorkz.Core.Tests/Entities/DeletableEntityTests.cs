using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Entities;

/// <summary>
/// Unit tests for the DeletableEntity&lt;TId&gt; and DeletableEntity base classes.
/// Tests soft delete functionality built on top of AuditEntity.
/// </summary>
public class DeletableEntityTests
{
    #region DeletableEntity<int> - Basic Soft Delete Property Tests

    [Fact]
    public void DeletableEntity_IsDeletedDefaultsFalse()
    {
        // Arrange
        var entity = new TestDeletableEntityInt();

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void DeletableEntity_DeletedAtDefaultsNull()
    {
        // Arrange
        var entity = new TestDeletableEntityInt();

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Null(deletedAt);
    }

    [Fact]
    public void DeletableEntity_DeletedByDefaultsNull()
    {
        // Arrange
        var entity = new TestDeletableEntityInt();

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Null(deletedBy);
    }

    [Fact]
    public void DeletableEntity_StoresIsDeleted()
    {
        // Arrange
        var entity = new TestDeletableEntityInt { IsDeleted = true };

        // Act
        var isDeleted = entity.IsDeleted;

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public void DeletableEntity_StoresDeletedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entity = new TestDeletableEntityInt { DeletedAt = now };

        // Act
        var deletedAt = entity.DeletedAt;

        // Assert
        Assert.Equal(now, deletedAt);
    }

    [Fact]
    public void DeletableEntity_StoresDeletedBy()
    {
        // Arrange
        var entity = new TestDeletableEntityInt { DeletedBy = 123 };

        // Act
        var deletedBy = entity.DeletedBy;

        // Assert
        Assert.Equal(123, deletedBy);
    }

    [Fact]
    public void DeletableEntity_AllPropertiesAreSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var deletedTime = DateTime.UtcNow.AddHours(1);
        var entity = new TestDeletableEntityInt
        {
            Id = 1,
            IsDeleted = true,
            DeletedAt = deletedTime,
            DeletedBy = 999
        };

        // Act & Assert
        Assert.Equal(1, entity.Id);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedTime, entity.DeletedAt);
        Assert.Equal(999, entity.DeletedBy);
    }

    #endregion

    #region DeletableEntity Interface Implementation Tests

    [Fact]
    public void DeletableEntity_ImplementsISoftDeletable()
    {
        // Arrange
        var entity = new TestDeletableEntityInt();

        // Act & Assert
        Assert.IsAssignableFrom<ISoftDeletable>(entity);
    }

    [Fact]
    public void DeletableEntity_ImplementsISoftDeletableInterface_HasSoftDeleteProperties()
    {
        // Arrange
        var entity = new TestDeletableEntityInt() as ISoftDeletable;

        // Act & Assert
        Assert.NotNull(entity);
        Assert.True(entity.GetType().GetProperty("IsDeleted") != null);
        Assert.True(entity.GetType().GetProperty("DeletedAt") != null);
        Assert.True(entity.GetType().GetProperty("DeletedBy") != null);
    }

    #endregion

    #region DeletableEntity Inheritance Tests

    [Fact]
    public void DeletableEntity_InheritsFromAuditEntity()
    {
        // Arrange
        var entity = new TestDeletableEntityInt { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<AuditEntity<int>>(entity);
        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void DeletableEntity_InheritsFromEntity()
    {
        // Arrange
        var entity = new TestDeletableEntityInt { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void DeletableEntity_InheritanceChain_EntityAuditEntityDeletableEntity()
    {
        // Arrange
        var entity = new TestDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddHours(1),
            DeletedBy = 456
        };

        // Act & Assert
        Assert.IsAssignableFrom<Entity<int>>(entity);
        Assert.IsAssignableFrom<AuditEntity<int>>(entity);
        Assert.IsAssignableFrom<DeletableEntity<int>>(entity);
        Assert.Equal(1, entity.Id);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(456, entity.DeletedBy);
    }

    [Fact]
    public void DeletableEntity_PreservesEntityEqualitySemantics()
    {
        // Arrange
        var entity1 = new TestDeletableEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = false
        };
        var entity2 = new TestDeletableEntityInt
        {
            Id = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "bob",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = "admin"
        };

        // Act & Assert
        // Two deletable entities with same Id are equal, regardless of soft delete properties
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void DeletableEntity_DifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestDeletableEntityInt
        {
            Id = 5,
            IsDeleted = false
        };
        var entity2 = new TestDeletableEntityInt
        {
            Id = 10,
            IsDeleted = false
        };

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.False(entity1 == entity2);
    }

    #endregion

    #region DeletableEntity Generic Type Tests

    [Fact]
    public void DeletableEntity_SupportsGuidPrimaryKey()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var deletedTime = DateTime.UtcNow;
        var entity = new TestDeletableEntityGuid
        {
            Id = guid,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user123",
            IsDeleted = true,
            DeletedAt = deletedTime,
            DeletedBy = 789
        };

        // Act & Assert
        Assert.Equal(guid, entity.Id);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedTime, entity.DeletedAt);
        Assert.Equal(789, entity.DeletedBy);
    }

    [Fact]
    public void DeletableEntity_SupportsStringPrimaryKey()
    {
        // Arrange
        var entity = new TestDeletableEntityString
        {
            Id = "CODE123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user456",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = 111
        };

        // Act & Assert
        Assert.Equal("CODE123", entity.Id);
        Assert.True(entity.IsDeleted);
        Assert.Equal(111, entity.DeletedBy);
    }

    #endregion

    #region DeletableEntity Convenience Class Tests

    [Fact]
    public void DeletableEntity_ConvenienceClass_HasIntId()
    {
        // Arrange
        var entity = new TestDeletableEntity { Id = 100, CreatedAt = DateTime.UtcNow };

        // Act & Assert
        Assert.Equal(100, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void DeletableEntity_ConvenienceClass_InheritsFromDeletableEntityInt()
    {
        // Arrange & Act
        var entity = new TestDeletableEntity { Id = 50 };

        // Assert
        Assert.IsAssignableFrom<DeletableEntity<int>>(entity);
    }

    [Fact]
    public void DeletableEntity_ConvenienceClass_HasSoftDeleteProperties()
    {
        // Arrange
        var deletedTime = DateTime.UtcNow;
        var entity = new TestDeletableEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = true,
            DeletedAt = deletedTime,
            DeletedBy = 555
        };

        // Act & Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedTime, entity.DeletedAt);
        Assert.Equal(555, entity.DeletedBy);
    }

    #endregion

    #region Soft Delete with Audit Trail Tests

    [Fact]
    public void DeletableEntity_DeletedEntity_MaintainsAuditTrail()
    {
        // Arrange
        var createdTime = DateTime.UtcNow;
        var updatedTime = DateTime.UtcNow.AddHours(1);
        var deletedTime = DateTime.UtcNow.AddHours(2);
        var entity = new TestDeletableEntityInt
        {
            Id = 1,
            CreatedAt = createdTime,
            CreatedBy = "alice",
            UpdatedAt = updatedTime,
            UpdatedBy = "bob",
            IsDeleted = true,
            DeletedAt = deletedTime,
            DeletedBy = 999
        };

        // Act & Assert
        // Verify all audit properties are preserved after soft delete
        Assert.Equal(createdTime, entity.CreatedAt);
        Assert.Equal("alice", entity.CreatedBy);
        Assert.Equal(updatedTime, entity.UpdatedAt);
        Assert.Equal("bob", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedTime, entity.DeletedAt);
        Assert.Equal(999, entity.DeletedBy);
    }

    [Fact]
    public void DeletableEntity_CanTransitionFromActiveToDeleted()
    {
        // Arrange
        var entity = new TestDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = false
        };

        // Act - Transition to deleted state
        var deletedTime = DateTime.UtcNow.AddHours(1);
        entity.IsDeleted = true;
        entity.DeletedAt = deletedTime;
        entity.DeletedBy = 888;

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedTime, entity.DeletedAt);
        Assert.Equal(888, entity.DeletedBy);
    }

    [Fact]
    public void DeletableEntity_DeletedAtAndDeletedByAreNullable()
    {
        // Arrange
        var entity = new TestDeletableEntityInt
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "alice",
            IsDeleted = true,
            DeletedAt = null,
            DeletedBy = null
        };

        // Act & Assert
        Assert.True(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);
    }

    #endregion

    #region Test Entities

    /// <summary>
    /// Test deletable entity with int primary key.
    /// </summary>
    private class TestDeletableEntityInt : DeletableEntity<int>
    {
    }

    /// <summary>
    /// Test deletable entity with Guid primary key.
    /// </summary>
    private class TestDeletableEntityGuid : DeletableEntity<Guid>
    {
    }

    /// <summary>
    /// Test deletable entity with string primary key.
    /// </summary>
    private class TestDeletableEntityString : DeletableEntity<string>
    {
    }

    /// <summary>
    /// Test convenience deletable entity inheriting from DeletableEntity (int primary key).
    /// </summary>
    private class TestDeletableEntity : DeletableEntity
    {
    }

    #endregion
}
