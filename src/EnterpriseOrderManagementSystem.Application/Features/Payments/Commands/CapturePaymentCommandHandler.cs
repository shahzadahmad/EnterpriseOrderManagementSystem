using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CapturePaymentCommandHandler
    : IRequestHandler<CapturePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;

    public CapturePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Guid> Handle(
        CapturePaymentCommand request,
        CancellationToken cancellationToken)
    {
        #region Load Payment

        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(
                "Payment",
                request.PaymentId);
        }

        #endregion

        #region Capture Through Gateway

        var gatewayResult = await _paymentGateway.CaptureAsync(
            new PaymentCaptureRequest(
                payment.Id,
                payment.ProviderReference!,
                payment.Amount,
                payment.Currency),
            cancellationToken);

        #endregion

        #region Handle Gateway Failure

        if (!gatewayResult.IsSuccessful)
        {
            payment.Fail(
                gatewayResult.FailureCode ?? "PAYMENT_CAPTURE_FAILED",
                gatewayResult.FailureReason ?? "Payment capture failed.");

            return payment.Id;
        }

        #endregion

        #region Update Domain

        var providerReference =
            gatewayResult.ProviderReference
            ?? payment.ProviderReference!;

        payment.Capture(providerReference);

        #endregion

        return payment.Id;
    }
}