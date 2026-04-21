namespace SmartWorkz.ECommerce.Mobile;

public sealed class CartViewModel : ViewModelBase
{
    public ObservableCollection<ProductDto> CartItems { get; } = new();

    public void AddToCart(ProductDto product, int quantity)
    {
        // Check if product already in cart
        var existingItem = CartItems.FirstOrDefault(p => p.Id == product.Id);
        if (existingItem != null)
        {
            // In a real implementation, you would update the quantity
            // For now, just clear and re-add
            CartItems.Remove(existingItem);
        }

        CartItems.Add(product);
    }
}
