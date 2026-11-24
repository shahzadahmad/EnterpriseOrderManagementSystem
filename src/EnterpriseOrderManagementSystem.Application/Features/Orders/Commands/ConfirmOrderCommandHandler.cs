using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using InvtAggreate = EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Features.Orders.Commands;

public sealed class ConfirmOrderCommandHandler
    : IRequestHandler<ConfirmOrderCommand, Guid>
{
    #region Fields

    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;


    #endregion

    #region Constructor

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository, 
        IInventoryRepository inventoryRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
    }

    #endregion

    #region Handle

    public async Task<Guid> Handle(
        ConfirmOrderCommand request,
        CancellationToken cancellationToken)
    {
        #region Retrieve and Validate Order

        var order = await _orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            throw new NotFoundException(
                "Order",
                request.OrderId);

        if (order.Items.Count == 0)
            throw new ConflictException(
                "An order must contain at least one item before confirmation.");

        #endregion

        #region Validate Inventory Availability

        // Validate every inventory record and quantity before changing stock.
        var inventories = new List<(InvtAggreate.Inventory Inventory, int Quantity)>();

        foreach (var item in order.Items)
        {
            var inventory = await _inventoryRepository
                .GetByProductAndWarehouseAsync(
                    item.ProductId,
                    order.WarehouseId,
                    cancellationToken);

            if (inventory is null)
                throw new ConflictException(
                    $"No inventory exists for product '{item.ProductId}' " +
                    $"in warehouse '{order.WarehouseId}'.");

            if (inventory.IsDiscontinued)
                throw new ConflictException(
                    $"Inventory for product '{item.ProductName}' is discontinued.");

            if (inventory.AvailableQuantity < item.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for '{item.ProductName}'. " +
                    $"Available: {inventory.AvailableQuantity}; requested: {item.Quantity}.");

            inventories.Add((inventory, item.Quantity));
        }

        #endregion

        #region Reserve Inventory Stock

        // All checks passed. Reserve stock for every line.
        foreach (var entry in inventories)
        {
            entry.Inventory.ReserveStock(
                entry.Quantity,
                order.OrderNumber,
                $"Stock reserved for order {order.OrderNumber}");
        }

        #endregion

        #region Confirm Order

        order.Confirm();

        return order.Id;

        #endregion

    }

    #endregion
}
