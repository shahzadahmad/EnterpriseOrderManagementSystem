using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class SettlePaymentCommandHandler
    : IRequestHandler<SettlePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public SettlePaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Guid> Handle(
        SettlePaymentCommand request,
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

        #region Settle Payment

        payment.Settle(
            payment.ProviderReference!);

        #endregion

        return payment.Id;
    }
}