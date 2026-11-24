
using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Extensions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class CapturePaymentCommandHandler
    : IRequestHandler<CapturePaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;

    #endregion

    #region Constructor

    public CapturePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CapturePaymentCommand request,
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
        if (payment.Status != PaymentStatus.Authorized)
        {
            throw new ConflictException(
                $"Only authorized payments can be captured. Current status: {payment.Status}.");
        }

        // Validate provider reference
        if (string.IsNullOrWhiteSpace(payment.ProviderReference))
        {
            throw new ConflictException(
                "Payment cannot be captured without a provider reference.");
        }

        #endregion

        #region Capture Payment Through Gateway

        // Send capture request to payment gateway
        var gatewayResult = await _paymentGateway.CaptureAsync(
            new PaymentCaptureRequest(
                payment.Id,
                payment.ProviderReference,
                payment.Amount,
                payment.Currency),
            cancellationToken);

        #endregion

        #region Handle Gateway Failure

        // Handle unsuccessful gateway response
        if (!gatewayResult.IsSuccessful)
        {
            var failureCode = gatewayResult.FailureCode
                ?? "PAYMENT_PROCESSING_FAILED";

            var failureReason = gatewayResult.FailureReason
                ?? "Payment processing failed.";

            // Mark payment as failed
            payment.Fail(failureCode, failureReason);

            // Load related order
            var order = await _orderRepository.GetByIdAsync(
                payment.OrderId,
                cancellationToken);

            // Release reserved inventory and cancel order
            if (order is not null &&
                order.Status == OrderStatus.Confirmed)
            {
                #region Released Reserved Inventory

                foreach (var item in order.Items)
                {
                    // Load inventory for the order's product and warehouse
                    var inventory = await _inventoryRepository
                        .GetByProductAndWarehouseAsync(
                            item.ProductId,
                            order.WarehouseId,
                            cancellationToken);

                    // Validate reserved stock before releasing it
                    if (inventory is null ||
                        inventory.ReservedQuantity < item.Quantity)
                    {
                        throw new ConflictException(
                            $"Could not release the reservation for '{item.ProductName}' " +
                            "after payment failure.");
                    }

                    // Release reserved stock
                    inventory.ReleaseReservedStock(
                        item.Quantity,
                        order.OrderNumber,
                        $"Stock released after payment failure for order {order.OrderNumber}");
                }

                #endregion

                #region Cancel Order

                // Cancel order after releasing reserved inventory
                order.Cancel(
                    $"Payment failed: {failureReason}".Truncate(500));

                #endregion
            }

            return payment.Id;
        }

        #endregion

        #region Update Payment and Return Result

        // Get provider reference from gateway response
        var providerReference =
            gatewayResult.ProviderReference
            ?? payment.ProviderReference;

        // Mark payment as captured
        payment.Capture(providerReference);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
