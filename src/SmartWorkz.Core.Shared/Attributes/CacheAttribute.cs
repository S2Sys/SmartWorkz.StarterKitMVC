using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace SmartWorkz.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CacheAttribute : TypeFilterAttribute
{
    public CacheAttribute() : base(typeof(CacheFilter))
    {
        Arguments = new object[] { this };
    }

    public int Seconds { get; set; } = 60;
    public string? Key { get; set; }
    public bool SlidingExpiration { get; set; } = false;
}

internal class CacheFilter(IMemoryCache cache, CacheAttribute options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var key = options.Key ?? context.HttpContext.Request.Path.ToString();
        if (cache.TryGetValue(key, out var cached))
        {
            context.Result = new OkObjectResult(cached);
            return;
        }
        var executed = await next();
        if (executed.Result is ObjectResult { Value: not null } result)
        {
            var expiry = options.SlidingExpiration
                ? new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(options.Seconds))
                : new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Seconds));
            cache.Set(key, result.Value, expiry);
        }
    }
}
