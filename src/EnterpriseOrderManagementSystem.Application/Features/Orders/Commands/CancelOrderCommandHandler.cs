using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using InvAggregate = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _paymentRepository = paymentRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        #region Retrieve Order and Validate Payment

        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Order", request.OrderId);

        var payment = await _paymentRepository.GetByOrderIdAsync(
            order.Id,
            cancellationToken);

        if (payment is not null &&
            payment.Status is PaymentStatus.Captured or PaymentStatus.Settled)
        {
            throw new ConflictException(
                "A captured or settled payment must be refunded before the order can be cancelled.");
        }

        #endregion

        #region Release Reserved Inventory

        // A pending order has not reserved stock.
        if (order.Status == OrderStatus.Confirmed)
        {
            var inventories = new List<(InvAggregate.Inventory Inventory, int Quantity)>();

            foreach (var item in order.Items)
            {
                var inventory = await _inventoryRepository
                    .GetByProductAndWarehouseAsync(
                        item.ProductId,
                        order.WarehouseId,
                        cancellationToken);

                if (inventory is null ||
                    inventory.ReservedQuantity < item.Quantity)
                {
                    throw new ConflictException(
                        $"Cannot safely release the reservation for '{item.ProductName}'.");
                }

                inventories.Add((inventory, item.Quantity));
            }

            foreach (var entry in inventories)
            {
                entry.Inventory.ReleaseReservedStock(
                    entry.Quantity,
                    order.OrderNumber,
                    $"Reservation released because order {order.OrderNumber} was cancelled");
            }
        }

        #endregion

        #region Cancel Pending or Authorized Payment

        if (payment is not null &&
            payment.Status is PaymentStatus.Pending or PaymentStatus.Authorized)
        {
            payment.Cancel($"Order cancelled: {request.Reason}");
        }

        #endregion

        #region Cancel Order

        order.Cancel(request.Reason);

        return order.Id;

        #endregion        
    }

    #endregion
}