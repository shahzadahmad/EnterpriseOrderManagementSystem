using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Determines how inventory should be allocated
/// for an order.
///
/// This service coordinates Order and Inventory information
/// but does not modify either aggregate.
/// </summary>
public sealed class InventoryAllocationDomainService : IInventoryAllocationDomainService
{
    #region Public Methods

    /// <summary>
    /// Determines the inventory allocation required
    /// for every item in the order.
    /// </summary>
    public IReadOnlyCollection<InventoryAllocation> Allocate(
        Order order,
        IReadOnlyCollection<Inventory> inventories)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(inventories);

        #region Validate Order

        if (order.Items.Count == 0)
        {
            throw new BusinessRuleViolationException(
                "An order must contain at least one item.");
        }

        #endregion

        #region Create Allocation

        var allocations = new List<InventoryAllocation>();

        foreach (var orderItem in order.Items)
        {
            var inventory = inventories.FirstOrDefault(x => x.ProductId == orderItem.ProductId);

            if (inventory is null)
            {
                throw new BusinessRuleViolationException(
                    $"No inventory was found for product '{orderItem.ProductId}'.");
            }

            if (inventory.AvailableQuantity < orderItem.Quantity)
            {
                throw new BusinessRuleViolationException(
                    $"Insufficient inventory for product '{orderItem.ProductId}'. " +
                    $"Required: {orderItem.Quantity}, " +
                    $"Available: {inventory.AvailableQuantity}.");
            }

            allocations.Add(
                new InventoryAllocation(
                    orderItem.ProductId,
                    inventory.Id,
                    orderItem.Quantity));
        }

        #endregion

        return allocations;
    }

    #endregion
}