using SmartWorkz.Extensions;
using Microsoft.Extensions.Logging;
using SmartWorkz.Models;

namespace SmartWorkz.Clients;

/// <summary>
/// Client for the Products API endpoint.
///
/// <para><strong>Endpoints</strong>:
/// • GET /api/products - List products (paginated)
/// • GET /api/products/{id} - Get product by ID
/// • POST /api/products - Create new product
/// • PUT /api/products/{id} - Update product
/// • DELETE /api/products/{id} - Delete product
/// </para>
///
/// <para><strong>Usage Examples</strong>:
/// <code>
/// // List products with price filtering
/// var listRequest = new ListProductsRequest(
///     PageNumber: 1,
///     PageSize: 20,
///     SearchTerm: "laptop",
///     MinPrice: 500m,
///     MaxPrice: 2000m);
/// var products = await client.Products.ListAsync(listRequest);
/// Console.WriteLine($"Found {products.TotalCount} products");
///
/// // Get specific product
/// var product = await client.Products.GetAsync("prod-789");
/// Console.WriteLine($"{product.Name} - ${product.Price}");
/// Console.WriteLine($"Stock: {product.StockQuantity} units");
///
/// // Create product
/// var createRequest = new CreateProductRequest(
///     Name: "USB-C Cable",
///     Description: "High-speed USB-C charging cable",
///     Price: 29.99m,
///     StockQuantity: 100);
/// var created = await client.Products.CreateAsync(createRequest);
/// </code>
/// </para>
/// </summary>
public interface IProductsClient
{
    /// <summary>
    /// Lists all products with pagination and optional filtering.
    /// </summary>
    /// <param name="request">List query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated product list.</returns>
    Task<PaginatedResponse<GetProductResponse>> ListAsync(ListProductsRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product information.</returns>
    Task<GetProductResponse> GetAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">Product creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created product information.</returns>
    Task<GetProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The product ID to update.</param>
    /// <param name="request">Product update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated product information.</returns>
    Task<GetProductResponse> UpdateAsync(string productId, UpdateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="productId">The product ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion response.</returns>
    Task<ApiResponse<string>> DeleteAsync(string productId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IProductsClient"/>.
/// </summary>
public class ProductsClient : IProductsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsClient>? _logger;

    public ProductsClient(HttpClient httpClient, ILogger<ProductsClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResponse<GetProductResponse>> ListAsync(ListProductsRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ListProductsRequest();

        var queryParams = BuildQueryString(request);
        var url = $"/api/products{queryParams}";

        _logger?.LogDebug("Listing products from {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<PaginatedResponse<GetProductResponse>>(cancellationToken);
    }

    public async Task<GetProductResponse> GetAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentNullException(nameof(productId));

        var url = $"/api/products/{Uri.EscapeDataString(productId)}";
        _logger?.LogDebug("Getting product {ProductId}", productId);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<GetProductResponse>(cancellationToken);
    }

    public async Task<GetProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger?.LogInformation("Creating product: {ProductName} - Price: {Price}", request.Name, request.Price);

        var response = await _httpClient.PostAsJsonAsync("/api/products", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadAsAsync<GetProductResponse>(cancellationToken);
        _logger?.LogInformation("Product created successfully: {ProductId}", created.Id);

        return created;
    }

    public async Task<GetProductResponse> UpdateAsync(string productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentNullException(nameof(productId));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var url = $"/api/products/{Uri.EscapeDataString(productId)}";
        _logger?.LogInformation("Updating product {ProductId}", productId);

        var response = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadAsAsync<GetProductResponse>(cancellationToken);
        _logger?.LogInformation("Product updated successfully: {ProductId}", productId);

        return updated;
    }

    public async Task<ApiResponse<string>> DeleteAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentNullException(nameof(productId));

        var url = $"/api/products/{Uri.EscapeDataString(productId)}";
        _logger?.LogInformation("Deleting product {ProductId}", productId);

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger?.LogInformation("Product deleted successfully: {ProductId}", productId);

        return new ApiResponse<string>(Success: true, Data: productId, Message: "Product deleted successfully");
    }

    private static string BuildQueryString(ListProductsRequest request)
    {
        var queries = new List<string>();

        if (request.PageNumber > 0)
            queries.Add($"pageNumber={request.PageNumber}");

        if (request.PageSize > 0)
            queries.Add($"pageSize={request.PageSize}");

        if (!string.IsNullOrEmpty(request.SearchTerm))
            queries.Add($"search={Uri.EscapeDataString(request.SearchTerm)}");

        if (request.MinPrice.HasValue)
            queries.Add($"minPrice={request.MinPrice}");

        if (request.MaxPrice.HasValue)
            queries.Add($"maxPrice={request.MaxPrice}");

        return queries.Count > 0 ? $"?{string.Join("&", queries)}" : string.Empty;
    }
}
