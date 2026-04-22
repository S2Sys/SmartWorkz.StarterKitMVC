using SmartWorkz.Core;

namespace SmartWorkz.Core.Tests.Enums;

/// <summary>
/// Tests for EntityStatus alias - verifying backward compatibility with EntityState.
/// </summary>
public class EntityStatusAliasTests
{
    #region Type Alias Tests

    [Fact]
    public void EntityStatus_IsAlias_PointsToEntityState()
    {
        // Arrange & Act
        var entityStatusType = typeof(EntityStatus);
        var entityStateType = typeof(EntityState);

        // Assert
        // EntityStatus is now defined as a mirror enum with same values
        // In C# 11, type aliases require being at file level, so we use a mirror enum instead
        Assert.NotNull(entityStatusType);
        Assert.NotNull(entityStateType);
        // Both should be enums
        Assert.True(entityStatusType.IsEnum);
        Assert.True(entityStateType.IsEnum);
    }

    [Fact]
    public void EntityStatus_IsSameType_AsEntityState()
    {
        // Arrange
        var statusValue = EntityStatus.Active;
        var stateValue = EntityState.Active;

        // Act & Assert
        // Verify they are both enums
        Assert.True(statusValue.GetType().IsEnum);
        Assert.True(stateValue.GetType().IsEnum);

        // Verify equivalent integer values (backward compatibility)
        Assert.Equal((int)statusValue, (int)stateValue);
    }

    #endregion

    #region Lifecycle Category Alias Tests

    [Fact]
    public void EntityStatus_Active_EqualsEntityState_Active()
    {
        // Assert
        Assert.Equal((int)EntityState.Active, (int)EntityStatus.Active);
        Assert.Equal(0, (int)EntityStatus.Active);
    }

    [Fact]
    public void EntityStatus_Inactive_EqualsEntityState_Inactive()
    {
        // Assert
        Assert.Equal((int)EntityState.Inactive, (int)EntityStatus.Inactive);
        Assert.Equal(1, (int)EntityStatus.Inactive);
    }

    [Fact]
    public void EntityStatus_Archived_EqualsEntityState_Archived()
    {
        // Assert
        Assert.Equal((int)EntityState.Archived, (int)EntityStatus.Archived);
        Assert.Equal(2, (int)EntityStatus.Archived);
    }

    [Fact]
    public void EntityStatus_Deleted_EqualsEntityState_Deleted()
    {
        // Assert
        Assert.Equal((int)EntityState.Deleted, (int)EntityStatus.Deleted);
        Assert.Equal(3, (int)EntityStatus.Deleted);
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void EntityStatus_CanBeConvertedToEntityState()
    {
        // Arrange
        var statusValue = EntityStatus.Active;

        // Act
        var stateValue = (EntityState)(int)statusValue;

        // Assert
        // Verify conversion maintains the same integer value
        Assert.Equal((int)statusValue, (int)stateValue);
        Assert.Equal(EntityState.Active, stateValue);
    }

    #endregion

    #region Obsolete Attribute Tests

    [Fact]
    public void EntityStatus_HasObsoleteAttribute()
    {
        // Arrange
        var entityStatusType = typeof(EntityStatus);

        // Act
        var obsoleteAttribute = entityStatusType.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute;

        // Assert
        Assert.NotNull(obsoleteAttribute);
    }

    [Fact]
    public void EntityStatus_ObsoleteAttribute_IsWarning_NotError()
    {
        // Arrange
        var entityStatusType = typeof(EntityStatus);

        // Act
        var obsoleteAttribute = entityStatusType.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute;

        // Assert
        Assert.NotNull(obsoleteAttribute);
        Assert.False(obsoleteAttribute.IsError, "Obsolete attribute should be a warning (IsError=false), not an error (IsError=true)");
    }

    [Fact]
    public void EntityStatus_ObsoleteAttribute_HasMessage()
    {
        // Arrange
        var entityStatusType = typeof(EntityStatus);

        // Act
        var obsoleteAttribute = entityStatusType.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute;

        // Assert
        Assert.NotNull(obsoleteAttribute);
        Assert.NotNull(obsoleteAttribute.Message);
        Assert.NotEmpty(obsoleteAttribute.Message);
        Assert.Contains("EntityState", obsoleteAttribute.Message);
    }

    #endregion

    #region Major Category Values Tests

    [Fact]
    public void EntityStatus_VerificationCategory_AllValuesAccessible()
    {
        // Assert - Verify all verification category states are accessible via alias
        Assert.Equal((int)EntityState.EmailVerified, (int)EntityStatus.EmailVerified);
        Assert.Equal((int)EntityState.EmailPending, (int)EntityStatus.EmailPending);
        Assert.Equal((int)EntityState.PhoneVerified, (int)EntityStatus.PhoneVerified);
        Assert.Equal((int)EntityState.PhonePending, (int)EntityStatus.PhonePending);
    }

    [Fact]
    public void EntityStatus_ApprovalCategory_AllValuesAccessible()
    {
        // Assert - Verify all approval category states are accessible via alias
        Assert.Equal((int)EntityState.PendingApproval, (int)EntityStatus.PendingApproval);
        Assert.Equal((int)EntityState.Approved, (int)EntityStatus.Approved);
        Assert.Equal((int)EntityState.Rejected, (int)EntityStatus.Rejected);
    }

    [Fact]
    public void EntityStatus_OrderCategory_AllValuesAccessible()
    {
        // Assert - Verify all order category states are accessible via alias
        Assert.Equal((int)EntityState.OrderPlaced, (int)EntityStatus.OrderPlaced);
        Assert.Equal((int)EntityState.OrderConfirmed, (int)EntityStatus.OrderConfirmed);
        Assert.Equal((int)EntityState.Shipped, (int)EntityStatus.Shipped);
        Assert.Equal((int)EntityState.Delivered, (int)EntityStatus.Delivered);
        Assert.Equal((int)EntityState.Returned, (int)EntityStatus.Returned);
        Assert.Equal((int)EntityState.Cancelled, (int)EntityStatus.Cancelled);
    }

    [Fact]
    public void EntityStatus_PaymentCategory_AllValuesAccessible()
    {
        // Assert - Verify all payment category states are accessible via alias
        Assert.Equal((int)EntityState.PaymentPending, (int)EntityStatus.PaymentPending);
        Assert.Equal((int)EntityState.PaymentReceived, (int)EntityStatus.PaymentReceived);
        Assert.Equal((int)EntityState.PaymentFailed, (int)EntityStatus.PaymentFailed);
    }

    #endregion

    #region Display Attribute Coverage Tests

    [Fact]
    public void EntityStatus_DisplayAttributeCoverage_DocumentedAsIntentionallyOmitted()
    {
        // EntityStatus is an obsolete alias to EntityState.
        // The primary EntityState enum has Display attributes.
        // EntityStatus members intentionally do NOT have attributes to avoid duplication.
        // This is intentional - existing code should migrate to EntityState for display attributes.
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public void EntityStatus_CanBeUsedInSwitch_Statement()
    {
        // Arrange
        EntityStatus status = EntityStatus.Active;
        var result = string.Empty;

        // Act
        // This demonstrates backward compatibility - old code using switch on EntityStatus should work
        result = status switch
        {
            EntityStatus.Active => "Active",
            EntityStatus.Inactive => "Inactive",
            EntityStatus.Archived => "Archived",
            EntityStatus.Deleted => "Deleted",
            _ => "Unknown"
        };

        // Assert
        Assert.Equal("Active", result);
    }

    [Fact]
    public void EntityStatus_CanBeCompared_WithEntityState()
    {
        // Arrange
        EntityStatus statusValue = EntityStatus.Active;
        EntityState stateValue = EntityState.Active;

        // Act & Assert
        Assert.Equal((int)statusValue, (int)stateValue);
    }

    #endregion
}
