using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CancelPaymentCommandHandler
    : IRequestHandler<CancelPaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public CancelPaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Guid> Handle(
        CancelPaymentCommand request,
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

        #region Cancel Payment

        payment.Cancel(request.Reason);

        #endregion

        return payment.Id;
    }
}