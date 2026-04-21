namespace SmartWorkz.Mobile.Tests.Navigation;

public class NavigationParametersTests
{
    [Fact]
    public void Get_ExistingKey_ReturnsTypedValue()
    {
        var p = new NavigationParameters();
        p["productId"] = 42;

        var result = p.Get<int>("productId");

        Assert.Equal(42, result);
    }

    [Fact]
    public void Get_MissingKey_ReturnsDefault()
    {
        var p = new NavigationParameters();

        var result = p.Get<int>("missing");

        Assert.Equal(0, result);
    }

    [Fact]
    public void Contains_ExistingKey_ReturnsTrue()
    {
        var p = new NavigationParameters { ["key"] = "value" };

        Assert.True(p.Contains("key"));
    }

    [Fact]
    public void Contains_MissingKey_ReturnsFalse()
    {
        var p = new NavigationParameters();

        Assert.False(p.Contains("nope"));
    }

    [Fact]
    public void ToQueryString_BuildsQueryStringFromEntries()
    {
        var p = new NavigationParameters { ["id"] = 7, ["name"] = "Alice" };

        var qs = p.ToQueryString();

        Assert.Contains("id=7", qs);
        Assert.Contains("name=Alice", qs);
    }
}
