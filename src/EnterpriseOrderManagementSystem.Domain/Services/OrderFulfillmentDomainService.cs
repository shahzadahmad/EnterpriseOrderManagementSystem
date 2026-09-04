using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Aggregates.PaymentAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Services;

/// <summary>
/// Domain service responsible for determining whether
/// an order satisfies the business rules required for fulfillment.
///
/// This service does not modify any aggregate.
/// It only evaluates cross-aggregate business rules.
/// </summary>
public sealed class OrderFulfillmentDomainService
    : IOrderFulfillmentDomainService
{
    #region Public Methods

    /// <summary>
    /// Determines whether the supplied order can currently
    /// be fulfilled.
    ///
    /// Fulfillment requires:
    ///
    /// 1. A valid order.
    /// 2. A successful payment state.
    /// 3. All required inventory to be available.
    /// </summary>
    public bool CanFulfill(
        Order order,
        Payment payment,
        IReadOnlyCollection<Inventory> inventories)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(payment);

        ArgumentNullException.ThrowIfNull(inventories);

        #region Validate Order

        if (!CanOrderBeFulfilled(order))
        {
            return false;
        }

        #endregion

        #region Validate Payment

        if (!CanPaymentSupportFulfillment(payment))
        {
            return false;
        }

        #endregion

        #region Validate Inventory

        if (!HasRequiredInventory(
                order,
                inventories))
        {
            return false;
        }

        #endregion

        return true;
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    /// Determines whether the order is in a state
    /// that allows fulfillment.
    /// </summary>
    private static bool CanOrderBeFulfilled(
        Order order)
    {
        // Replace the status check below with the exact
        // OrderStatus values used by our Order aggregate
        // if the enum differs in your implementation.

        return order.Status == OrderStatus.Confirmed;
    }

    /// <summary>
    /// Determines whether the payment has reached
    /// a state where fulfillment is financially allowed.
    /// </summary>
    private static bool CanPaymentSupportFulfillment(
        Payment payment)
    {
        return payment.Status == PaymentStatus.Captured ||
               payment.Status == PaymentStatus.Settled;
    }

    /// <summary>
    /// Determines whether sufficient inventory exists
    /// for every order item.
    /// </summary>
    private static bool HasRequiredInventory(
        Order order,
        IReadOnlyCollection<Inventory> inventories)
    {
        foreach (var orderItem in order.Items)
        {
            var inventory = inventories.FirstOrDefault(
                x => x.ProductId == orderItem.ProductId);

            if (inventory is null)
            {
                return false;
            }

            if (inventory.AvailableQuantity < orderItem.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}