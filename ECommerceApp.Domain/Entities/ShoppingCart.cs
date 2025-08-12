namespace ECommerceApp.Domain.Entities;

public class ShoppingCart
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public List<CartItem> Items { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private ShoppingCart() { }
    public static ShoppingCart Create(string userId)
    {
        return new ShoppingCart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public void AddItem(Guid productId, int quantity, decimal price, string productName)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = CartItem.Create(productId, quantity, price, productName);
            Items.Add(newItem);
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
            }
            else
            {
                item.UpdateQuantity(quantity);
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
    public decimal GetTotal() => Items.Sum(item => item.Price * item.Quantity);
    public int GetTotalItems() => Items.Sum(item => item.Quantity);
}