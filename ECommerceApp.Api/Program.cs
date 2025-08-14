using System.Security.Claims;
using ECommerceApp.Infrastructure.Extensions;
using ECommerceApp.Api.Extensions;
using Carter;
using ECommerceApp.Application.Features.Products.Commands;
using ECommerceApp.Application.Features.Products.Queries;
using ECommerceApp.Application.Features.ShoppingCart.Commands;
using MediatR;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddCarter();

// Add authentication
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "E-Commerce API"));
}

app.UseAuthentication();
app.UseAuthorization();

// Map Carter modules
app.MapCarter();

app.Run();

// Product API Module
public class ProductModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get paginated products with filtering and sorting")
            .Produces<GetProductsResponse>()
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetProduct)
            .WithName("GetProduct")
            .WithSummary("Get product by ID")
            .Produces<ProductDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .RequireAuthorization("Admin")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/featured", GetFeaturedProducts)
            .WithName("GetFeaturedProducts")
            .WithSummary("Get featured products")
            .Produces<List<ProductDto>>();

        group.MapGet("/category/{categoryId:guid}", GetProductsByCategory)
            .WithName("GetProductsByCategory")
            .WithSummary("Get products by category")
            .Produces<List<ProductDto>>();
    }

    private static async Task<IResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProduct(
        Guid id,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        
        return result != null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.SKU,
            request.StockQuantity,
            request.CategoryId,
            request.Images);

        var result = await mediator.Send(command, cancellationToken);
        
        return Results.Created($"/api/products/{result.Id}", result);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        await mediator.Send(command, cancellationToken);
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        await mediator.Send(command, cancellationToken);
        
        return Results.NoContent();
    }

    private static async Task<IResult> GetFeaturedProducts(
        int count,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetFeaturedProductsQuery(count);
        var result = await mediator.Send(query, cancellationToken);
        
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProductsByCategory(
        Guid categoryId,
        int count,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsByCategoryQuery(categoryId, count);
        var result = await mediator.Send(query, cancellationToken);
        
        return Results.Ok(result);
    }
}


// Shopping Cart API Module
public class ShoppingCartModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Shopping Cart")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetCart)
            .WithName("GetCart")
            .WithSummary("Get user's shopping cart")
            .Produces<ShoppingCartDto>();

        group.MapPost("/items", AddToCart)
            .WithName("AddToCart")
            .WithSummary("Add item to cart")
            .Produces<AddToCartResponse>()
            .ProducesValidationProblem();

        group.MapPut("/items/{productId:guid}", UpdateCartItem)
            .WithName("UpdateCartItem")
            .WithSummary("Update cart item quantity")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapDelete("/items/{productId:guid}", RemoveFromCart)
            .WithName("RemoveFromCart")
            .WithSummary("Remove item from cart")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/", ClearCart)
            .WithName("ClearCart")
            .WithSummary("Clear all items from cart")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetCart(
        ClaimsPrincipal user,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = new GetShoppingCartQuery(userId!);
        var result = await mediator.Send(query, cancellationToken);
        
        return Results.Ok(result);
    }

    private static async Task<IResult> AddToCart(
        AddToCartRequest request,
        ClaimsPrincipal user,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new AddToCartCommand(userId!, request.ProductId, request.Quantity);
        var result = await mediator.Send(command, cancellationToken);
        
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateCartItem(
        Guid productId,
        UpdateCartItemRequest request,
        ClaimsPrincipal user,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new UpdateCartItemCommand(userId!, productId, request.Quantity);
        await mediator.Send(command, cancellationToken);
        
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFromCart(
        Guid productId,
        ClaimsPrincipal user,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new RemoveFromCartCommand(userId!, productId);
        await mediator.Send(command, cancellationToken);
        
        return Results.NoContent();
    }

    private static async Task<IResult> ClearCart(
        ClaimsPrincipal user,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new ClearCartCommand(userId!);
        await mediator.Send(command, cancellationToken);
        
        return Results.NoContent();
    }
}