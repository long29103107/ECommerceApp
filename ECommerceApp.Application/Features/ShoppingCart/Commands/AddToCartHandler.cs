using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Features.ShoppingCart.Commands;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, AddToCartResponse>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToCartHandler> _logger;
    public AddToCartHandler(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddToCartHandler> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<AddToCartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Get or create cart
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                  ?? ShoppingCart.Create(request.UserId);
        // Validate product
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return new AddToCartResponse(false, "Product not found", 0, 0);
        }
        if (!product.IsActive)
        {
            return new AddToCartResponse(false, "Product is not available", 0, 0);
        }
        if (product.StockQuantity < request.Quantity)
        {
            return new AddToCartResponse(false, $"Only {product.StockQuantity} items available", 0, 0);
        }
        // Add item to cart
        cart.AddItem(product.Id, request.Quantity, product.Price, product.Name);
        if (await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken) == null)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            _cartRepository.Update(cart);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added {Quantity} of product {ProductId} to cart for user {UserId}", 
            request.Quantity, request.ProductId, request.UserId);
        return new AddToCartResponse(
            true, 
            "Item added to cart successfully", 
            cart.GetTotalItems(), 
            cart.GetTotal());
    }
}