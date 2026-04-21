namespace SmartWorkz.Core.Tests.Utilities;

using System.ComponentModel;
using SmartWorkz.Shared.Utilities;

public class EnumHelperTests
{
    #region Test Enums

    public enum TestStatus
    {
        [Description("Active Status")]
        Active,

        [Description("Inactive Status")]
        Inactive,

        Pending
    }

    public enum ColorEnum
    {
        Red,
        Green,
        Blue
    }

    #endregion

    #region GetDescription Tests

    [Fact]
    public void GetDescription_WithDescriptionAttribute_ReturnsDescription()
    {
        // Act
        var result = EnumHelper.GetDescription(TestStatus.Active);

        // Assert
        Assert.Equal("Active Status", result);
    }

    [Fact]
    public void GetDescription_WithoutAttribute_ReturnsEnumName()
    {
        // Act
        var result = EnumHelper.GetDescription(TestStatus.Pending);

        // Assert
        Assert.Equal("Pending", result);
    }

    [Fact]
    public void GetDescription_WithDifferentEnumValue_ReturnsCorrectDescription()
    {
        // Act
        var result = EnumHelper.GetDescription(TestStatus.Inactive);

        // Assert
        Assert.Equal("Inactive Status", result);
    }

    #endregion

    #region GetValue Tests

    [Fact]
    public void GetValue_WithValidName_ReturnsSuccess()
    {
        // Act
        var result = EnumHelper.GetValue<TestStatus>("Active");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TestStatus.Active, result.Data);
    }

    [Fact]
    public void GetValue_WithCaseInsensitive_ReturnsSuccess()
    {
        // Act
        var result = EnumHelper.GetValue<TestStatus>("active");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TestStatus.Active, result.Data);
    }

    [Fact]
    public void GetValue_WithInvalidName_ReturnsFail()
    {
        // Act
        var result = EnumHelper.GetValue<TestStatus>("Invalid");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void GetValue_WithNullName_ReturnsFail()
    {
        // Act
        var result = EnumHelper.GetValue<TestStatus>(null!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void GetValue_WithEmptyName_ReturnsFail()
    {
        // Act
        var result = EnumHelper.GetValue<TestStatus>("");

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region GetAllValues Tests

    [Fact]
    public void GetAllValues_ReturnsAllEnumValues()
    {
        // Act
        var result = EnumHelper.GetAllValues<TestStatus>();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(TestStatus.Active, result);
        Assert.Contains(TestStatus.Inactive, result);
        Assert.Contains(TestStatus.Pending, result);
    }

    [Fact]
    public void GetAllValues_WithDifferentEnum_ReturnsCorrectValues()
    {
        // Act
        var result = EnumHelper.GetAllValues<ColorEnum>();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(ColorEnum.Red, result);
        Assert.Contains(ColorEnum.Green, result);
        Assert.Contains(ColorEnum.Blue, result);
    }

    #endregion

    #region GetName Tests

    [Fact]
    public void GetName_ReturnsEnumName()
    {
        // Act
        var result = EnumHelper.GetName(TestStatus.Active);

        // Assert
        Assert.Equal("Active", result);
    }

    [Fact]
    public void GetName_WithDifferentValues_ReturnsCorrectNames()
    {
        // Act & Assert
        Assert.Equal("Active", EnumHelper.GetName(TestStatus.Active));
        Assert.Equal("Inactive", EnumHelper.GetName(TestStatus.Inactive));
        Assert.Equal("Pending", EnumHelper.GetName(TestStatus.Pending));
    }

    #endregion
}

