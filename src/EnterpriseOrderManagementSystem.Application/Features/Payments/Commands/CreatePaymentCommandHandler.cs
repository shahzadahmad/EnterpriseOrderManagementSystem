using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CreatePaymentCommandHandler
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Guid> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        #region Validate Order

        var orderExists = await _orderRepository.ExistsAsync(
            request.OrderId,
            cancellationToken);

        if (!orderExists)
        {
            throw new NotFoundException(
                "Order",
                request.OrderId);
        }

        #endregion

        #region Validate Existing Payment

        var paymentExists = await _paymentRepository.ExistsByOrderIdAsync(
            request.OrderId,
            cancellationToken);

        if (paymentExists)
        {
            throw new ConflictException(
                $"A payment already exists for order '{request.OrderId}'.");
        }

        #endregion

        #region Create Payment

        var paymentId = Guid.NewGuid();

        var payment = Payment.Create(
            paymentId,
            request.OrderId,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.PaymentProvider);

        #endregion

        #region Persist

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        #endregion

        return payment.Id;
    }
}