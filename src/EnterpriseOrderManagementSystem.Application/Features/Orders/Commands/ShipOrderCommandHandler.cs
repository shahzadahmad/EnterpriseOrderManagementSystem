
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Enums;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using InvAggreate = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class ShipOrderCommandHandler
    : IRequestHandler<ShipOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPaymentRepository _paymentRepository;

    #endregion

    #region Constructor

    public ShipOrderCommandHandler(
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
        ShipOrderCommand request,
        CancellationToken cancellationToken)
    {
        #region Retrieve and Validate Order

        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            throw new NotFoundException("Order", request.OrderId);

        // Only confirmed orders can be shipped.
        if (order.Status != OrderStatus.Confirmed)
            throw new ConflictException(
                $"Only confirmed orders can be shipped. Current status: {order.Status}.");

        #endregion

        #region Validate Payment

        // An order must have a captured or settled payment before shipment.
        var payment = await _paymentRepository.GetByOrderIdAsync(
            order.Id,
            cancellationToken);

        if (payment is null ||
            payment.Status is not (
                PaymentStatus.Captured or
                PaymentStatus.Settled))
        {
            throw new ConflictException(
                "An order can only be shipped after its payment has been captured or settled.");
        }

        #endregion

        #region Validate Inventory Reservations

        var inventories = new List<(InvAggreate.Inventory Inventory, int Quantity)>();

        foreach (var item in order.Items)
        {
            var inventory = await _inventoryRepository
                .GetByProductAndWarehouseAsync(
                    item.ProductId,
                    order.WarehouseId,
                    cancellationToken);

            if (inventory is null)
                throw new ConflictException(
                    $"Inventory for product '{item.ProductId}' was not found.");

            if (inventory.ReservedQuantity < item.Quantity)
                throw new ConflictException(
                    $"Reserved quantity is insufficient for '{item.ProductName}'.");

            inventories.Add((inventory, item.Quantity));
        }

        #endregion

        #region Commit Inventory Reservations

        foreach (var entry in inventories)
        {
            entry.Inventory.CommitReservation(
                entry.Quantity,
                order.OrderNumber,
                $"Reserved stock committed for shipment of {order.OrderNumber}");
        }

        #endregion

        #region Ship Order

        order.Ship();

        return order.Id;

        #endregion
    }

    #endregion
}
