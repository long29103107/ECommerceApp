using MediatR;

namespace ECommerceApp.Application.Features.Products.Queries;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "Name",
    bool SortDescending = false
) : IRequest<GetProductsResponse>;
public record GetProductsResponse(
    List<ProductDto> Products,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string SKU,
    int StockQuantity,
    bool IsActive,
    CategoryDto Category,
    List<ProductImageDto> Images,
    decimal AverageRating,
    int ReviewCount
);