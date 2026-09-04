using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;
using EnterpriseOrderManagementSystem.Domain.Enums;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.OrderSpecifications;

/// <summary>
/// Determines whether an order is in the Confirmed state.
///
/// A confirmed order has passed the domain rules required
/// for confirmation and is eligible for the next stage
/// of the order lifecycle.
/// </summary>
public sealed class ConfirmedOrderSpecification : Specification<Order>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the supplied order is confirmed.
    /// </summary>
    public override bool IsSatisfiedBy(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Status == OrderStatus.Confirmed;
    }

    #endregion
}