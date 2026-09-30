using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class FailPaymentCommandHandler
    : IRequestHandler<FailPaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public FailPaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Guid> Handle(
        FailPaymentCommand request,
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

        #region Fail Payment

        payment.Fail(
            request.FailureCode,
            request.FailureReason);

        #endregion

        return payment.Id;
    }
}