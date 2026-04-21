using Xunit;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Grid;

public class GridRequestTests
{
    [Fact]
    public void GridRequest_ShouldInheritFromPagedQuery()
    {
        // Arrange
        var filters = new Dictionary<string, object> { { "Status", "Active" } };

        // Act
        var request = new GridRequest(Page: 2, PageSize: 50, SortBy: "Name", SortDescending: true, SearchTerm: "test", Filters: filters);

        // Assert
        Assert.Equal(2, request.Page);
        Assert.Equal(50, request.PageSize);
        Assert.Equal("Name", request.SortBy);
        Assert.True(request.SortDescending);
        Assert.Equal("test", request.SearchTerm);
        Assert.Single(request.Filters);
        Assert.Equal("Active", request.Filters["Status"]);
    }

    [Fact]
    public void GridRequest_ShouldHaveNullFiltersWhenNotProvided()
    {
        // Act
        var request = new GridRequest();

        // Assert
        Assert.Null(request.Filters);
        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public void GridRequest_ShouldCalculateSkipAndTake()
    {
        // Act
        var request = new GridRequest(Page: 3, PageSize: 25);

        // Assert
        Assert.Equal(50, request.Skip);  // (3-1) * 25
        Assert.Equal(25, request.Take);
    }
}


