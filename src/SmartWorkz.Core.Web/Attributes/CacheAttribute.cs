using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace SmartWorkz.Core.Web.Attributes;

/// <summary>Specifies response caching for MVC action methods and controllers.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CacheAttribute : TypeFilterAttribute
{
    /// <summary>Initializes a new instance of the <see cref="CacheAttribute"/> class.</summary>
    public CacheAttribute() : base(typeof(CacheFilter))
    {
        Arguments = new object[] { this };
    }

    /// <summary>Gets or sets the duration in seconds that the response is cached. Default is 60 seconds.</summary>
    public int Seconds { get; set; } = 60;

    /// <summary>Gets or sets the optional cache key. If null, uses the request path as the cache key.</summary>
    public string? Key { get; set; }

    /// <summary>Gets or sets a value indicating whether to use sliding expiration. If true, the expiration time extends with each access; if false, uses absolute expiration (default false).</summary>
    public bool SlidingExpiration { get; set; } = false;
}

internal class CacheFilter(IMemoryCache cache, CacheAttribute options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var key = GenerateCacheKey(options, context);
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

    private static string GenerateCacheKey(CacheAttribute options, ActionExecutingContext context)
    {
        if (!string.IsNullOrEmpty(options.Key))
            return options.Key;

        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var path = context.HttpContext.Request.Path.ToString();
        var queryString = context.HttpContext.Request.QueryString.ToString();

        return $"{userId}:{path}{queryString}";
    }
}
