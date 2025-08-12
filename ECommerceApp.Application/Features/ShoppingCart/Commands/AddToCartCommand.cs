using MediatR;

namespace ECommerceApp.Application.Features.ShoppingCart.Commands;

public record AddToCartCommand(
    string UserId,
    Guid ProductId,
    int Quantity
) : IRequest<AddToCartResponse>;
public record AddToCartResponse(
    bool Success,
    string Message,
    int TotalItems,
    decimal TotalAmount
);