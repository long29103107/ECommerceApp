namespace ECommerceApp.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    private OrderItem() { }
    public static OrderItem Create(Guid productId, int quantity, decimal price, string productName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Price = price,
            ProductName = productName
        };
    }
}