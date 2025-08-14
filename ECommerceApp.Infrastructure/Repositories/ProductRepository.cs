using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<ProductRepository> _logger;
    public ProductRepository(ECommerceDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    public async Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }
    public async Task<(List<Product> Products, int TotalCount)> GetPagedAsync(
        ProductSpecification specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .AsQueryable();
        // Apply specification
        query = specification.Apply(query);
        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (products, totalCount);
    }
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }
    public void Update(Product product)
    {
        _context.Products.Update(product);
    }
    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }
    public async Task<List<Product>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Product>> GetProductsByCategoryAsync(
        Guid categoryId, 
        int count, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
    public async Task<bool> IsStockAvailableAsync(
        Guid productId, 
        int requestedQuantity, 
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        return product?.StockQuantity >= requestedQuantity;
    }
}