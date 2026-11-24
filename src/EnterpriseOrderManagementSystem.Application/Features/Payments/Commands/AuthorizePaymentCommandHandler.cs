
using EnterpriseOrderManagementSystem.Application.Abstractions.Services.PaymentGateway;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Application.Common.Extensions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Payments.Commands;

public sealed class AuthorizePaymentCommandHandler
    : IRequestHandler<AuthorizePaymentCommand, Guid>
{
    #region Dependencies

    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;

    #endregion

    #region Constructor

    public AuthorizePaymentCommandHandler(
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
        AuthorizePaymentCommand request,
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
        if (payment.Status != PaymentStatus.Pending)
        {
            throw new ConflictException(
                $"Only pending payments can be authorized. Current status: {payment.Status}.");
        }

        #endregion

        #region Authorize Payment Through Gateway

        // Send authorization request to payment gateway
        var gatewayResult = await _paymentGateway.AuthorizeAsync(
            new PaymentAuthorizationRequest(
                payment.Id,
                payment.OrderId,
                payment.Amount,
                payment.Currency,
                payment.PaymentMethod.ToString()),
            cancellationToken);

        #endregion

        #region Handle Gateway Failure

        // Handle unsuccessful gateway response
        if (!gatewayResult.IsSuccessful)
        {
            var failureCode = gatewayResult.FailureCode
                ?? "PAYMENT_AUTHORIZATION_FAILED";

            var failureReason = gatewayResult.FailureReason
                ?? "Payment authorization failed.";

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
                #region Release Reserved Inventory

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
                            "after payment authorization failure.");
                    }

                    // Release reserved stock
                    inventory.ReleaseReservedStock(
                        item.Quantity,
                        order.OrderNumber,
                        $"Stock released after payment authorization failure for order {order.OrderNumber}");
                }

                #endregion

                #region Cancel Order

                // Cancel order after releasing reserved inventory
                order.Cancel(
                    $"Payment authorization failed: {failureReason}".Truncate(500));

                #endregion
            }

            return payment.Id;
        }

        #endregion

        #region Update Payment and Return Result

        // Validate gateway response
        if (string.IsNullOrWhiteSpace(gatewayResult.ProviderReference) ||
            string.IsNullOrWhiteSpace(gatewayResult.AuthorizationCode))
        {
            throw new ConflictException(
                "Payment gateway returned an invalid authorization response.");
        }

        // Mark payment as authorized
        payment.Authorize(
            gatewayResult.ProviderReference,
            gatewayResult.AuthorizationCode);

        // Return payment ID
        return payment.Id;

        #endregion
    }

    #endregion
}
