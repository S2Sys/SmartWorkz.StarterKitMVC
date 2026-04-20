        namespace SmartWorkz.Sample.ECommerce.Web.Models;

public class PagingModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalItems { get; set; }
    public int TotalPages => (TotalItems + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int PreviousPageNumber => PageNumber - 1;
    public int NextPageNumber => PageNumber + 1;
}

public class ProductListViewModel
{
    public List<SmartWorkz.Sample.ECommerce.Application.DTOs.ProductDto> Products { get; set; } = new();
    public PagingModel Paging { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } = "name"; // name, price-asc, price-desc
    public string? CategorySlug { get; set; }
}
