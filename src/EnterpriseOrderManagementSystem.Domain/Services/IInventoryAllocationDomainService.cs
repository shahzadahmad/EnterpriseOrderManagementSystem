using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Defines domain rules for allocating inventory
/// required by an order.
/// </summary>
public interface IInventoryAllocationDomainService
{
    /// <summary>
    /// Determines the inventory allocation required
    /// to fulfill the supplied order.
    ///
    /// The method does not modify inventory.
    /// </summary>
    IReadOnlyCollection<InventoryAllocation> Allocate(
        Order order,
        IReadOnlyCollection<Inventory> inventories);
}