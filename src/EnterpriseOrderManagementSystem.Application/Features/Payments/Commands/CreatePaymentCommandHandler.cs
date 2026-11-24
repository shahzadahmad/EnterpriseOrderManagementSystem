
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CreatePaymentCommandHandler
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    #endregion

    #region Constructor

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        #region Retrieve and Validate Order

        // Retrieve the order
        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "Order",
                request.OrderId);
        }

        // Only confirmed orders can have payments created
        if (order.Status != OrderStatus.Confirmed)
        {
            throw new ConflictException(
                $"Payment can only be created for a confirmed order. " +
                $"Current status: {order.Status}.");
        }

        // Ensure the order contains items and has a valid total
        if (order.Items.Count == 0 || order.TotalAmount.Amount <= 0)
        {
            throw new ConflictException(
                "Payment cannot be created for an empty or zero-value order.");
        }

        #endregion

        #region Validate Payment Amount and Currency

        // Validate payment amount
        if (request.Amount <= 0)
        {
            throw new ConflictException(
                "Payment amount must be greater than zero.");
        }

        if (request.Amount != order.TotalAmount.Amount)
        {
            throw new ConflictException(
                $"Payment amount must equal the order total. " +
                $"Expected: {order.TotalAmount.Amount}; received: {request.Amount}.");
        }

        // Validate payment currency
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ConflictException(
                "Payment currency is required.");
        }

        if (!string.Equals(
                request.Currency,
                order.TotalAmount.Currency,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"Payment currency must match the order currency. " +
                $"Expected: {order.TotalAmount.Currency}; received: {request.Currency}.");
        }

        #endregion

        #region Validate Payment Method and Provider

        // Validate payment method
        if (!Enum.IsDefined(request.PaymentMethod))
        {
            throw new ConflictException(
                "The selected payment method is invalid.");
        }

        // Validate payment provider
        if (!Enum.IsDefined(request.PaymentProvider) ||
            request.PaymentProvider == PaymentProvider.None)
        {
            throw new ConflictException(
                "The selected payment provider is invalid.");
        }

        #endregion

        #region Check Existing Payment

        // Prevent multiple payment records for the same order
        var paymentExists = await _paymentRepository.ExistsByOrderIdAsync(
            request.OrderId,
            cancellationToken);

        if (paymentExists)
        {
            throw new ConflictException(
                $"A payment already exists for order '{request.OrderId}'.");
        }

        #endregion

        #region Create and Persist Payment

        // Use the order total as the authoritative payment amount and currency
        var payment = Payment.Create(
            Guid.NewGuid(),
            order.Id,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            request.PaymentMethod,
            request.PaymentProvider);

        // Persist payment
        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        // Return newly created payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
