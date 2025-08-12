namespace ECommerceApp.Application.Features.Products.Queries;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetProductsHandler> _logger;
    public GetProductsHandler(
        IProductRepository productRepository,
        IMemoryCache cache,
        ILogger<GetProductsHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }
    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request);
        
        if (_cache.TryGetValue(cacheKey, out GetProductsResponse? cachedResponse))
        {
            _logger.LogInformation("Products retrieved from cache");
            return cachedResponse!;
        }
        var specification = new ProductSpecification(
            request.SearchTerm,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.SortBy,
            request.SortDescending);
        var (products, totalCount) = await _productRepository.GetPagedAsync(
            specification,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.SKU,
            p.StockQuantity,
            p.IsActive,
            new CategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug),
            p.Images.Select(img => new ProductImageDto(img.Id, img.Url, img.AltText, img.IsPrimary)).ToList(),
            p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            p.Reviews.Count
        )).ToList();
        var response = new GetProductsResponse(
            productDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            (int)Math.Ceiling((double)totalCount / request.PageSize));
        // Cache for 5 minutes
        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
        return response;
    }
    private static string GenerateCacheKey(GetProductsQuery request)
    {
        return $"products_{request.PageNumber}_{request.PageSize}_{request.SearchTerm}_{request.CategoryId}_{request.MinPrice}_{request.MaxPrice}_{request.SortBy}_{request.SortDescending}";
    }
}