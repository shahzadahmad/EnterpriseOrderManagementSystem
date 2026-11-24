
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CancelPaymentCommandHandler
    : IRequestHandler<CancelPaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public CancelPaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CancelPaymentCommand request,
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

        // Validate cancellation reason
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ConflictException(
                "Payment cancellation reason is required.");
        }

        #endregion

        #region Cancel Payment and Return Result

        // Cancel payment using the domain method.
        // The domain entity enforces valid payment state transitions.
        payment.Cancel(request.Reason);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
