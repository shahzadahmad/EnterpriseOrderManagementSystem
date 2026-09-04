using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.InventorySpecifications;

/// <summary>
/// Determines whether an inventory record has
/// available stock.
/// </summary>
public sealed class InventoryHasAvailableStockSpecification
    : Specification<Inventory>
{
    #region Evaluation

    /// <summary>
    /// Determines whether available inventory is greater than zero.
    /// </summary>
    public override bool IsSatisfiedBy(Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return inventory.AvailableQuantity > 0;
    }

    #endregion
}