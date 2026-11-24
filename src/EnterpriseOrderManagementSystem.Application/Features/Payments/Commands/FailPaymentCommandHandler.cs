
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class FailPaymentCommandHandler
    : IRequestHandler<FailPaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public FailPaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        FailPaymentCommand request,
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

        // Validate failure code
        if (string.IsNullOrWhiteSpace(request.FailureCode))
        {
            throw new ConflictException(
                "Payment failure code is required.");
        }

        // Validate failure reason
        if (string.IsNullOrWhiteSpace(request.FailureReason))
        {
            throw new ConflictException(
                "Payment failure reason is required.");
        }

        #endregion

        #region Fail Payment and Return Result

        // Mark payment as failed
        payment.Fail(
            request.FailureCode,
            request.FailureReason);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
