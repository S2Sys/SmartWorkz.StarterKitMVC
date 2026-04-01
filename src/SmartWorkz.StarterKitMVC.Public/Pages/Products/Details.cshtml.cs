using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Products
{
    public class DetailsModel : PageModel
    {
        public string? ProductId { get; set; }

        public void OnGet(string? id)
        {
            ProductId = id;
        }
    }
}
