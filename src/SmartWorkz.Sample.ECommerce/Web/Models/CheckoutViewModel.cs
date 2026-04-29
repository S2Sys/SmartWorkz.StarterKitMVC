using SmartWorkz.Sample.ECommerce.Application.DTOs;

namespace SmartWorkz.Sample.ECommerce.Web.Models;

public class CheckoutViewModel
{
    public CartDto Cart { get; set; } = new(new());
    public CheckoutDto Checkout { get; set; } = new("", "", "", "", "");
}
