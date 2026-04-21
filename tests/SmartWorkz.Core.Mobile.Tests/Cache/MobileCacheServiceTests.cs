namespace SmartWorkz.Mobile.Tests.Cache;

using Moq;
using SmartWorkz.Shared;

public class MobileCacheServiceTests
{
    private readonly Mock<IOfflineService> _offline = new();

    [Fact]
    public async Task GetOrSetAsync_CacheMiss_InvokesFactory()
    {
        _offline.Setup(o => o.GetFromCacheAsync<string>("k", default))
                .ReturnsAsync(Result.Fail<string>(new Error("CACHE.MISS", "not found")));
        _offline.Setup(o => o.CacheAsync("k", "hello", It.IsAny<TimeSpan?>(), default))
                .ReturnsAsync(Result.Ok());

        var svc = new MobileCacheService(_offline.Object);

        var result = await svc.GetOrSetAsync("k", () => Task.FromResult("hello"));

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task GetOrSetAsync_CacheHit_DoesNotInvokeFactory()
    {
        _offline.Setup(o => o.GetFromCacheAsync<string>("k", default))
                .ReturnsAsync(Result.Ok("cached"));

        bool factoryCalled = false;
        var svc = new MobileCacheService(_offline.Object);

        var result = await svc.GetOrSetAsync("k", () => { factoryCalled = true; return Task.FromResult("new"); });

        Assert.Equal("cached", result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task RemoveAsync_DelegatesToOfflineService()
    {
        _offline.Setup(o => o.GetFromCacheAsync<object>("k", default))
                .ReturnsAsync(Result.Fail<object>(new Error("CACHE.MISS", "not found")));
        var svc = new MobileCacheService(_offline.Object);

        await svc.RemoveAsync("k");
    }
}
