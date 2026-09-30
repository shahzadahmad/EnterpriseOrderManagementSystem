namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway
{
    public interface IPaymentGateway
    {
        Task<PaymentAuthorizationResult> AuthorizeAsync(
            PaymentAuthorizationRequest request,
            CancellationToken cancellationToken = default);

        Task<PaymentCaptureResult> CaptureAsync(
            PaymentCaptureRequest request,
            CancellationToken cancellationToken = default);

        Task<PaymentRefundResult> RefundAsync(
            PaymentRefundRequest request,
            CancellationToken cancellationToken = default);
    }
}
