
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class SettlePaymentCommandHandler
    : IRequestHandler<SettlePaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public SettlePaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        SettlePaymentCommand request,
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

        // Validate payment status
        if (payment.Status != PaymentStatus.Captured)
        {
            throw new ConflictException(
                $"Only captured payments can be settled. Current status: {payment.Status}.");
        }

        // Validate provider reference
        if (string.IsNullOrWhiteSpace(payment.ProviderReference))
        {
            throw new ConflictException(
                "Payment cannot be settled without a provider reference.");
        }

        #endregion

        #region Settle Payment and Return Result

        // Mark payment as settled
        payment.Settle(payment.ProviderReference);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
