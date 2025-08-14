namespace ECommerceApp.Infrastructure.Services;


public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken);
    Task<PaymentIntentResponse> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken);
    Task<RefundResponse> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken);
}