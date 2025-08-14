using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Specifications;

public class ProductSpecification
{
    public string? SearchTerm { get; }
    public Guid? CategoryId { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }
    public string SortBy { get; }
    public bool SortDescending { get; }
    public ProductSpecification(
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "Name",
        bool sortDescending = false)
    {
        SearchTerm = searchTerm;
        CategoryId = categoryId;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }
    public IQueryable<Product> Apply(IQueryable<Product> query)
    {
        // Apply filters
        query = query.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(SearchTerm) || 
                                     p.Description.Contains(SearchTerm));
        }
        if (CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == CategoryId.Value);
        }
        if (MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= MinPrice.Value);
        }
        if (MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= MaxPrice.Value);
        }
        // Apply sorting
        query = SortBy.ToLowerInvariant() switch
        {
            "name" => SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "created" => SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };
        return query;
    }
}