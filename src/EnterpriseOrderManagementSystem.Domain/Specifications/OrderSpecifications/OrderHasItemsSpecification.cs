using EnterpriseOrderManagementSystem.Domain.Aggregates.OrderAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.OrderSpecifications;

/// <summary>
/// Determines whether an order contains at least one
/// order item.
/// </summary>
public sealed class OrderHasItemsSpecification
    : Specification<Order>
{
    #region Evaluation

    /// <summary>
    /// Determines whether the order contains one or more items.
    /// </summary>
    public override bool IsSatisfiedBy(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Items.Count > 0;
    }

    #endregion
}