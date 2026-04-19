using Xunit;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Tests.Grid;

public class GridColumnTests
{
    [Fact]
    public void GridColumn_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var column = new GridColumn { PropertyName = "Name", DisplayName = "Product Name" };

        // Assert
        Assert.True(column.IsSortable);
        Assert.True(column.IsFilterable);
        Assert.False(column.IsEditable);
        Assert.True(column.IsVisible);
        Assert.Equal(0, column.Order);
        Assert.Null(column.FilterType);
    }

    [Fact]
    public void GridColumn_ShouldValidatePropertyNameRequired()
    {
        // Act & Assert
        var column = new GridColumn { DisplayName = "Test" };
        Assert.Null(column.PropertyName);
    }

    [Fact]
    public void GridColumn_ShouldHaveValidFilterTypes()
    {
        // Arrange
        var filterTypes = new[] { "text", "dropdown", "date", "range" };

        // Act & Assert
        foreach (var type in filterTypes)
        {
            var column = new GridColumn { PropertyName = "Name", FilterType = type };
            Assert.Equal(type, column.FilterType);
        }
    }
}
