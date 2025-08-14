using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Specifications;

namespace ECommerceApp.Infrastructure.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);

    Task<(List<Product> Products, int TotalCount)> GetPagedAsync(
        ProductSpecification specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    public void Update(Product product);
    public void Delete(Product product);
    Task<List<Product>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken = default);

    Task<List<Product>> GetProductsByCategoryAsync(
        Guid categoryId,
        int count,
        CancellationToken cancellationToken = default);

    Task<bool> IsStockAvailableAsync(
        Guid productId,
        int requestedQuantity,
        CancellationToken cancellationToken = default);
}