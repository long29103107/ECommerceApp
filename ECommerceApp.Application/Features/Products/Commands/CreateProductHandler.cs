using ECommerceApp.Domain.Entities;
using MediatR;

namespace ECommerceApp.Application.Features.Products.Commands;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductHandler> _logger;
    public CreateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProductHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            throw new ArgumentException($"Category with ID {request.CategoryId} not found");
        // Check SKU uniqueness
        var existingProduct = await _productRepository.GetBySKUAsync(request.SKU, cancellationToken);
        if (existingProduct != null)
            throw new ArgumentException($"Product with SKU {request.SKU} already exists");
        // Create product
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.SKU,
            request.StockQuantity,
            request.CategoryId);
        // Add images
        foreach (var imageDto in request.Images)
        {
            var image = ProductImage.Create(imageDto.Url, imageDto.AltText, imageDto.IsPrimary);
            product.Images.Add(image);
        }
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        return new CreateProductResponse(
            product.Id,
            product.Name,
            product.Price,
            product.SKU,
            product.CreatedAt);
    }
}