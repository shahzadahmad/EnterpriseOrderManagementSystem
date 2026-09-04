using EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;

namespace EnterpriseOrderManagementSystem.Domain.Specifications.InventorySpecifications;

/// <summary>
/// Determines whether an inventory record has sufficient
/// available quantity to reserve a requested amount.
/// </summary>
public sealed class InventoryCanReserveSpecification
    : Specification<Inventory>
{
    #region Fields

    private readonly int _quantity;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes the specification with the quantity
    /// that needs to be reserved.
    /// </summary>
    public InventoryCanReserveSpecification(
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleViolationException(
                "Reservation quantity must be greater than zero.");
        }

        _quantity = quantity;
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Determines whether the inventory has enough
    /// available quantity for the requested reservation.
    /// </summary>
    public override bool IsSatisfiedBy(
        Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return inventory.AvailableQuantity >= _quantity;
    }

    #endregion
}