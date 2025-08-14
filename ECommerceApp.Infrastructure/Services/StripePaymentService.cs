using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerceApp.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IOptions<StripeSettings> stripeSettings, ILogger<StripePaymentService> logger)
    {
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency ?? "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = request.OrderId.ToString(),
                    ["user_id"] = request.UserId
                },
                ReceiptEmail = request.CustomerEmail,
                Description = $"Order {request.OrderId}",
                Shipping = new ChargeShippingOptions
                {
                    Name = request.ShippingAddress.Name,
                    Address = new AddressOptions
                    {
                        Line1 = request.ShippingAddress.Street,
                        City = request.ShippingAddress.City,
                        State = request.ShippingAddress.State,
                        PostalCode = request.ShippingAddress.ZipCode,
                        Country = request.ShippingAddress.Country
                    }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment intent created: {PaymentIntentId} for order: {OrderId}", 
                paymentIntent.Id, request.OrderId);

            return new PaymentIntentResponse
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = request.Amount
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent for order: {OrderId}", request.OrderId);
            throw new PaymentProcessingException($"Payment processing failed: {ex.Message}", ex);
        }
    }
public async Task<PaymentIntentResponse> ConfirmPaymentAsync(
        string paymentIntentId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            return new PaymentIntentResponse
            {
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount / 100m // Convert from cents
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment: {PaymentIntentId}", paymentIntentId);
            throw new PaymentProcessingException($"Payment confirmation failed: {ex.Message}", ex);
        }
    }

    public async Task<RefundResponse> RefundPaymentAsync(
        RefundRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = request.PaymentIntentId,
                Amount = request.Amount.HasValue ? (long)(request.Amount.Value * 100) : null,
                Reason = request.Reason,
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = request.OrderId.ToString(),
                    ["refund_reason"] = request.Reason ?? "requested_by_customer"
                }
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Refund created: {RefundId} for payment: {PaymentIntentId}", 
                refund.Id, request.PaymentIntentId);

            return new RefundResponse
            {
                RefundId = refund.Id,
                Status = refund.Status,
                Amount = refund.Amount / 100m
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating refund for payment: {PaymentIntentId}", request.PaymentIntentId);
            throw new PaymentProcessingException($"Refund processing failed: {ex.Message}", ex);
        }
    }
}

public class PaymentProcessingException : Exception
{
    public PaymentProcessingException(string message) : base(message) { }
    public PaymentProcessingException(string message, Exception innerException) : base(message, innerException) { }
}

// Data Models
public class CreatePaymentIntentRequest
{
    public Guid OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
}

public class PaymentIntentResponse
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class RefundRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class RefundResponse
{
    public string RefundId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ShippingAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}