using System.Linq.Expressions;
using System.Text.Json;
using SmartWorkz.Shared.Grid;
using SmartWorkz.Shared.Results;
using SmartWorkz.Shared.Pagination;

namespace SmartWorkz.Web;

/// <summary>
/// Web-specific implementation of grid data fetching via HTTP API or in-memory sources.
/// </summary>
public class GridDataProvider : IGridDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public GridDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Fetch data from HTTP API endpoint.
    /// </summary>
    public async Task<SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>> GetDataAsync<T>(GridRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/grid/data", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = new SmartWorkz.Core.Shared.Results.Error(
                    "GridDataFetchFailed",
                    $"API returned status {response.StatusCode}");
                return SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>.Fail<GridResponse<T>>(error);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GridResponse<T>>(responseContent, _jsonOptions);

            return result != null
                ? SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>.Ok<GridResponse<T>>(result)
                : SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>.Fail<GridResponse<T>>(new SmartWorkz.Core.Shared.Results.Error("DeserializationFailed", "Could not parse grid response"));
        }
        catch (HttpRequestException ex)
        {
            return SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>.Fail<GridResponse<T>>(new SmartWorkz.Core.Shared.Results.Error("HttpError", ex.Message));
        }
        catch (Exception ex)
        {
            return SmartWorkz.Core.Shared.Results.Result<GridResponse<T>>.Fail<GridResponse<T>>(new SmartWorkz.Core.Shared.Results.Error("UnexpectedError", ex.Message));
        }
    }

    /// <summary>
    /// Apply sorting, filtering, and paging to an in-memory IEnumerable.
    /// Used when grid is bound to local data instead of an API.
    /// </summary>
    public static PagedList<T> ApplyGridLogic<T>(
        IEnumerable<T> source,
        GridRequest request)
    {
        var query = source.AsQueryable();

        // Apply search term (across all string properties)
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = ApplySearch(query, request.SearchTerm);
        }

        // Apply filters
        if (request.Filters?.Any() == true)
        {
            query = ApplyFilters(query, request.Filters);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            query = (request.SortDescending
                ? query.OrderByDescending(CreatePropertySelector<T>(request.SortBy))
                : query.OrderBy(CreatePropertySelector<T>(request.SortBy))).AsQueryable();
        }

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply pagination
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return PagedList<T>.Create(items, request.Page, request.PageSize, totalCount);
    }

    private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm)
    {
        // Simple implementation: search across string properties
        var properties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .ToList();

        if (!properties.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? predicate = null;

        foreach (var prop in properties)
        {
            var property = Expression.Property(parameter, prop.Name);
            var constant = Expression.Constant(searchTerm);
            var contains = Expression.Call(property, "Contains", null, constant);

            predicate = predicate == null
                ? contains
                : Expression.OrElse(predicate, contains);
        }

        if (predicate == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<T> ApplyFilters<T>(
        IQueryable<T> query,
        Dictionary<string, object> filters)
    {
        foreach (var filter in filters)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, filter.Key);
            var constant = Expression.Constant(filter.Value);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            query = query.Where(lambda);
        }

        return query;
    }

    private static Func<T, object?> CreatePropertySelector<T>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda<Func<T, object?>>(
            Expression.Convert(property, typeof(object)),
            parameter);
        return lambda.Compile();
    }
}
