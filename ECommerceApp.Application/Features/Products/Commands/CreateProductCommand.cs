using MediatR;

namespace ECommerceApp.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string SKU,
    int StockQuantity,
    Guid CategoryId,
    List<CreateProductImageDto> Images
) : IRequest<CreateProductResponse>;

public record CreateProductImageDto(string Url, string AltText, bool IsPrimary);
public record CreateProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string SKU,
    DateTime CreatedAt
);