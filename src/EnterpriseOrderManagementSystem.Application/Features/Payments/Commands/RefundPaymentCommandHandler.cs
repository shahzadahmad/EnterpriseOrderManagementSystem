using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class RefundPaymentCommandHandler
    : IRequestHandler<RefundPaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Guid> Handle(
        RefundPaymentCommand request,
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

        #region Refund Through Gateway

        var gatewayResult = await _paymentGateway.RefundAsync(
            new PaymentRefundRequest(
                payment.Id,
                payment.ProviderReference!,
                request.RefundAmount,
                payment.Currency),
            cancellationToken);

        #endregion

        #region Handle Gateway Failure

        if (!gatewayResult.IsSuccessful)
        {
            throw new ConflictException(
                gatewayResult.FailureReason
                ?? "Payment refund failed.");
        }

        #endregion

        #region Update Domain

        payment.Refund(
            request.RefundAmount,
            gatewayResult.ProviderReference!,
            request.Reason);

        #endregion

        return payment.Id;
    }
}