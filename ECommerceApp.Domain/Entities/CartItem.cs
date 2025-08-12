namespace ECommerceApp.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public DateTime AddedAt { get; private set; }
    private CartItem() { }
    public static CartItem Create(Guid productId, int quantity, decimal price, string productName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        return new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Price = price,
            ProductName = productName,
            AddedAt = DateTime.UtcNow
        };
    }
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
        
        Quantity = newQuantity;
    }
}