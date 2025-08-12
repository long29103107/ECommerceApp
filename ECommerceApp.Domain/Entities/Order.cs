using ECommerceApp.Domain.Enums;

namespace ECommerceApp.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public List<OrderItem> Items { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private Order() { }
    public static Order Create(
        string userId,
        Address shippingAddress,
        Address billingAddress,
        List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (!items.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(items));
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Items = items,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        order.CalculateTotals();
        return order;
    }
    private void CalculateTotals()
    {
        SubTotal = Items.Sum(item => item.Price * item.Quantity);
        TaxAmount = SubTotal * 0.1m; // 10% tax
        ShippingAmount = CalculateShipping();
        TotalAmount = SubTotal + TaxAmount + ShippingAmount;
    }
    private decimal CalculateShipping()
    {
        // Simple shipping calculation - can be enhanced
        return SubTotal > 100 ? 0 : 10;
    }
    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}