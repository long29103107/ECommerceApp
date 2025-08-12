namespace ECommerceApp.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CategoryId { get; private set; }  
    public Category Category { get; private set; } = null!;
    public List<ProductImage> Images { get; private set; } = new();
    public List<ProductReview> Reviews { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    private Product() { } // EF Core constructor
    public static Product Create(
        string name, 
        string description, 
        decimal price, 
        string sku, 
        int stockQuantity, 
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        
        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(price));
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            SKU = sku,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));
        
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    public bool IsInStock() => StockQuantity > 0;
    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");
        
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}