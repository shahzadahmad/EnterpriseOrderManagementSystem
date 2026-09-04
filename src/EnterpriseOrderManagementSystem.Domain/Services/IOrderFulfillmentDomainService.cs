using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Defines domain rules for determining whether an order
/// can currently be fulfilled.
///
/// The service coordinates information from multiple
/// aggregates without owning their state.
/// </summary>
public interface IOrderFulfillmentDomainService
{
    /// <summary>
    /// Determines whether the supplied order can be fulfilled.
    /// </summary>
    /// <param name="order">
    /// Order being evaluated.
    /// </param>
    /// <param name="payment">
    /// Payment associated with the order.
    /// </param>
    /// <param name="inventories">
    /// Inventory records required by the order.
    /// </param>
    /// <returns>
    /// True when all fulfillment rules are satisfied;
    /// otherwise false.
    /// </returns>
    bool CanFulfill(
        Order order,
        Payment payment,
        IReadOnlyCollection<Inventory> inventories);
}