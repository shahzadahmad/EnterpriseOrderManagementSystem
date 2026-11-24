
using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class RefundPaymentCommandHandler
    : IRequestHandler<RefundPaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;

    #endregion

    #region Constructor

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        RefundPaymentCommand request,
        CancellationToken cancellationToken)
    {
        #region Retrieve and Validate Payment

        // Load payment
        var payment = await _paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(
                "Payment",
                request.PaymentId);
        }

        // Validate payment provider reference
        if (string.IsNullOrWhiteSpace(payment.ProviderReference))
        {
            throw new ConflictException(
                "Payment cannot be refunded without a provider reference.");
        }

        // Validate refund amount
        if (request.RefundAmount <= 0)
        {
            throw new ConflictException(
                "Refund amount must be greater than zero.");
        }

        // Validate refundable payment status
        if (payment.Status != PaymentStatus.Captured &&
            payment.Status != PaymentStatus.Settled &&
            payment.Status != PaymentStatus.PartiallyRefunded)
        {
            throw new ConflictException(
                $"Payment cannot be refunded. Current status: {payment.Status}.");
        }

        #endregion

        #region Refund Payment Through Gateway

        // Send refund request to payment gateway
        var gatewayResult = await _paymentGateway.RefundAsync(
            new PaymentRefundRequest(
                payment.Id,
                payment.ProviderReference,
                request.RefundAmount,
                payment.Currency),
            cancellationToken);

        #endregion

        #region Handle Gateway Failure

        // Handle unsuccessful gateway response
        if (!gatewayResult.IsSuccessful)
        {
            throw new ConflictException(
                gatewayResult.FailureReason
                ?? "Payment refund failed.");
        }

        #endregion

        #region Update Payment and Return Result

        // Validate gateway response
        if (string.IsNullOrWhiteSpace(gatewayResult.ProviderReference))
        {
            throw new ConflictException(
                "Payment gateway returned an invalid refund response.");
        }

        // Update payment refund details
        payment.Refund(
            request.RefundAmount,
            gatewayResult.ProviderReference,
            request.Reason);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
