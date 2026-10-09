using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.Payment;

public sealed class MockPaymentGateway
    : IPaymentGateway
{
    #region Authorize

    public Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var providerReference =
            $"MOCK-AUTH-{Guid.NewGuid():N}";

        var authorizationCode =
            $"AUTH-{Random.Shared.Next(100000, 999999)}";

        var result =
            new PaymentAuthorizationResult(
                true,
                providerReference,
                authorizationCode,
                null,
                null);

        return Task.FromResult(result);
    }

    #endregion

    #region Capture

    public Task<PaymentCaptureResult> CaptureAsync(
        PaymentCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var providerReference =
            $"MOCK-CAP-{Guid.NewGuid():N}";

        var result =
            new PaymentCaptureResult(
                true,
                providerReference,
                null,
                null);

        return Task.FromResult(result);
    }

    #endregion

    #region Refund

    public Task<PaymentRefundResult> RefundAsync(
        PaymentRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var refundReference =
            $"MOCK-REF-{Guid.NewGuid():N}";

        var result =
            new PaymentRefundResult(
                true,
                refundReference,
                null,
                null);

        return Task.FromResult(result);
    }

    #endregion
}