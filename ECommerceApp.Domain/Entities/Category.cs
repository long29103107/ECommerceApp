namespace ECommerceApp.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public List<Product> Products { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    private Category() { }
    public static Category Create(string name, string description, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };
    }
}